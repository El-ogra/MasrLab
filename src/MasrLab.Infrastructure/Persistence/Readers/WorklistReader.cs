using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;
using MasrLab.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

public sealed class WorklistReader : IWorklistReader
{
    private readonly MasrLabDbContext _context;

    public WorklistReader(MasrLabDbContext context) => _context = context;

    public async Task<IReadOnlyList<WorklistPatientDto>> GetAsync(
        DateTime date,
        AccountType? category,
        CancellationToken cancellationToken = default)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        // OQ-M4-10: category filter reads registration-time Patient.AccountType.
        var visits = await (from visit in _context.PatientVisits.AsNoTracking()
                join patient in _context.Patients.AsNoTracking()
                    on visit.PatientId equals patient.Id
                where visit.VisitDate >= dayStart && visit.VisitDate < dayEnd
                where category == null || patient.AccountType == category
                orderby visit.Id
                select new
                {
                    VisitId = visit.Id,
                    PatientId = patient.Id,
                    PatientName = patient.Name,
                    visit.LabId,
                    AccountType = patient.AccountType
                })
            .ToListAsync(cancellationToken);
        if (visits.Count == 0)
            return [];

        var visitIds = visits.Select(v => v.VisitId).ToList();

        var tests = await _context.VisitTests.AsNoTracking()
            .Where(vt => visitIds.Contains(vt.PatientVisitId))
            .OrderBy(vt => vt.Id)
            .Select(vt => new
            {
                vt.Id,
                vt.PatientVisitId,
                vt.TestId,
                vt.TestNameSnapshot,
                vt.IsFinished,
                vt.IsVerified,
                vt.IsPrinted,
                vt.IsExportMarked
            })
            .ToListAsync(cancellationToken);

        var testIds = tests.Select(t => t.TestId).Distinct().ToList();
        var testInfos = await _context.Tests.AsNoTracking()
            .Where(t => testIds.Contains(t.Id))
            .Select(t => new { t.Id, t.TestCode, t.SeeReport })
            .ToDictionaryAsync(t => t.Id, t => (t.TestCode, t.SeeReport), cancellationToken);

        var resultsByVisitTest = await (from item in _context.VisitTestResultItems.AsNoTracking()
                join result in _context.TestResults.AsNoTracking()
                    on item.Id equals result.VisitTestResultItemId into items
                from result in items.DefaultIfEmpty()
                where visitIds.Contains(item.VisitTestId) && !item.IsDeleted && (result == null || !result.IsDeleted)
                group new { result!.Value, result.Status } by item.VisitTestId
                into g
                select new
                {
                    VisitTestId = g.Key,
                    Values = g.Where(x => x.Value != null).Select(x => x.Value).ToList(),
                    Statuses = g.Select(x => x.Status).ToList()
                })
            .ToDictionaryAsync(x => x.VisitTestId, cancellationToken);

        return visits
            .Select(v => new WorklistPatientDto(
                v.VisitId,
                v.PatientId,
                v.PatientName,
                v.LabId,
                v.AccountType,
                tests
                    .Where(t => t.PatientVisitId == v.VisitId)
                    .Select(t =>
                    {
                        var info = testInfos.TryGetValue(t.TestId, out var found) ? found : default;
                        resultsByVisitTest.TryGetValue(t.Id, out var aggregated);
                        var hasResults = aggregated is { Values.Count: > 0 };
                        var status = ResolveStatus(aggregated?.Statuses);
                        return new WorklistTestRowDto(
                            t.Id,
                            t.TestId,
                            string.IsNullOrEmpty(info.TestCode) ? t.TestNameSnapshot : info.TestCode,
                            info.SeeReport
                                ? "See Report"
                                : hasResults ? string.Join(" | ", aggregated!.Values) : string.Empty,
                            status,
                            t.IsFinished,
                            t.IsVerified,
                            t.IsPrinted,
                            t.IsExportMarked);
                    })
                    .ToList()))
            .ToList();
    }

    private static string? ResolveStatus(List<ResultStatus>? statuses)
    {
        if (statuses is not { Count: > 0 })
            return null;
        if (statuses.Any(s => s == ResultStatus.High))
            return ResultStatus.High.ToString();
        if (statuses.Any(s => s == ResultStatus.Low))
            return ResultStatus.Low.ToString();
        return statuses.All(s => s == ResultStatus.Normal)
            ? ResultStatus.Normal.ToString()
            : null;
    }
}
