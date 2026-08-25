using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.ResultsEntry.Queries.GetConsolidatedReport;
using MasrLab.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

public sealed class ClinicalReportReader : IClinicalReportReader
{
    private readonly MasrLabDbContext _context;

    public ClinicalReportReader(MasrLabDbContext context) => _context = context;

    public async Task<ConsolidatedReportDto?> GetConsolidatedAsync(
        int consolidatedReportId,
        CancellationToken cancellationToken = default)
    {
        var report = await _context.ConsolidatedReports.AsNoTracking()
            .Where(r => r.Id == consolidatedReportId)
            .Select(r => new
            {
                r.Id,
                r.PatientVisitId,
                r.PrintGroupSubtitles,
                r.Comment
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (report is null)
            return null;

        var header = await (
                from visit in _context.PatientVisits.AsNoTracking()
                join patient in _context.Patients.AsNoTracking()
                    on visit.PatientId equals patient.Id
                where visit.Id == report.PatientVisitId
                select new { patient.Name, visit.LabId, visit.VisitDate })
            .SingleOrDefaultAsync(cancellationToken);
        if (header is null)
            return null;

        // Composition order (M4-BR-11): items in DisplayOrder sequence.
        var items = await _context.ConsolidatedReportItems.AsNoTracking()
            .Where(i => i.ConsolidatedReportId == consolidatedReportId)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new { i.VisitTestId })
            .ToListAsync(cancellationToken);
        var visitTestIds = items.Select(i => i.VisitTestId).ToList();

        var tests = await _context.VisitTests.AsNoTracking()
            .Where(vt => visitTestIds.Contains(vt.Id))
            .Select(vt => new
            {
                vt.Id,
                vt.ReportNameSnapshot,
                vt.TestNameSnapshot,
                GroupTitle = vt.TestGroupNameSnapshot ?? string.Empty,
                ResultItems = vt.ResultItems
                    .Where(ri => !ri.IsDeleted && ri.IncludeInPrint) // OQ-M4-5 inclusion flags.
                    .OrderBy(ri => ri.DisplayOrder)
                    .Select(ri => ri.Id)
                    .ToList()
            })
            .ToListAsync(cancellationToken);
        var testsById = tests.ToDictionary(t => t.Id);

        var itemIds = tests.SelectMany(t => t.ResultItems).ToList();
        var resultsByItemId = await (
                from result in _context.TestResults.AsNoTracking()
                where itemIds.Contains(result.VisitTestResultItemId) && !result.IsDeleted
                select new { result.VisitTestResultItemId, result.Value, result.Unit, result.ReferenceRange, result.Status })
            .ToDictionaryAsync(r => r.VisitTestResultItemId, cancellationToken);

        var lines = new List<ConsolidatedReportLineDto>();
        string? lastSubtitle = null;
        foreach (var item in items)
        {
            if (!testsById.TryGetValue(item.VisitTestId, out var visitTest))
                continue; // deleted test — silently dropped from rendering.

            if (report.PrintGroupSubtitles && !string.IsNullOrWhiteSpace(visitTest.GroupTitle)
                && !string.Equals(lastSubtitle, visitTest.GroupTitle, StringComparison.Ordinal))
            {
                lastSubtitle = visitTest.GroupTitle;
                lines.Add(ConsolidatedReportLineDto.Subtitle(visitTest.GroupTitle));
            }

            var testName = string.IsNullOrWhiteSpace(visitTest.ReportNameSnapshot)
                ? visitTest.TestNameSnapshot
                : visitTest.ReportNameSnapshot;

            var enteredRows = 0;
            foreach (var resultItemId in visitTest.ResultItems)
            {
                if (!resultsByItemId.TryGetValue(resultItemId, out var result))
                    continue;
                enteredRows++;
                lines.Add(new ConsolidatedReportLineDto(
                    testName,
                    result.Value,
                    result.Unit,
                    result.ReferenceRange,
                    result.Status == ResultStatus.Normal ? string.Empty : result.Status.ToString()));
            }

            // OQ-M4-14 binding rule: an un-entered test prints blank with the placeholder.
            if (enteredRows == 0)
                lines.Add(ConsolidatedReportLineDto.NotEntered(testName));
        }

        return new ConsolidatedReportDto(
            report.Id,
            report.PatientVisitId,
            header.Name,
            header.LabId,
            header.VisitDate,
            report.PrintGroupSubtitles,
            report.Comment,
            lines);
    }
}
