using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Application.Features.ResultsEntry.Queries.GetConsolidatedReport;
using MasrLab.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

// Slice 11: builds inclusion-flagged print payloads for all four report kinds
// without mutating anything. Reuses the Slice 8/9/10 readers where possible.
public sealed class VisitReportPrintReader : IVisitReportPrintReader
{
    private readonly MasrLabDbContext _context;
    private readonly IClinicalReportReader _clinicalReportReader;
    private readonly IBlankReportReader _blankReportReader;
    private readonly IMicrobiologyReportReader _microbiologyReportReader;

    public VisitReportPrintReader(
        MasrLabDbContext context,
        IClinicalReportReader clinicalReportReader,
        IBlankReportReader blankReportReader,
        IMicrobiologyReportReader microbiologyReportReader)
    {
        _context = context;
        _clinicalReportReader = clinicalReportReader;
        _blankReportReader = blankReportReader;
        _microbiologyReportReader = microbiologyReportReader;
    }

    public async Task<VisitReportPrintData?> GetAsync(
        int visitTestId,
        VisitReportKind kind,
        int? reportId,
        CancellationToken cancellationToken = default)
    {
        var visitTest = await _context.VisitTests.AsNoTracking()
            .Where(vt => vt.Id == visitTestId)
            .Select(vt => new { vt.Id, vt.PatientVisitId })
            .FirstOrDefaultAsync(cancellationToken);
        if (visitTest is null)
            return null;

        var header = await BuildHeaderAsync(visitTest.PatientVisitId, cancellationToken);
        if (header is null)
            return null;

        List<ClinicalResultLineDto> lines;
        DateTime? lastPrintedAt = null;
        int? lastPrintedByUserId = null;
        var enteredResultIds = new List<int>();
        int? cultureItemId = null;

        switch (kind)
        {
            case VisitReportKind.Individual:
                (lines, lastPrintedAt, lastPrintedByUserId, enteredResultIds) =
                    await BuildIndividualAsync(visitTestId, header.Value.VisitDate, cancellationToken);
                break;

            case VisitReportKind.Consolidated:
                (lines, lastPrintedAt, lastPrintedByUserId) =
                    await BuildConsolidatedAsync(reportId ?? 0, cancellationToken);
                break;

            case VisitReportKind.Blank:
                (lines, lastPrintedAt, lastPrintedByUserId) =
                    await BuildBlankAsync(reportId ?? 0, cancellationToken);
                break;

            case VisitReportKind.Culture:
                (lines, cultureItemId, lastPrintedAt, lastPrintedByUserId) =
                    await BuildCultureAsync(visitTestId, cancellationToken);
                break;

            default:
                lines = new List<ClinicalResultLineDto>();
                break;
        }

        var payload = new ClinicalReportPrintDto
        {
            PatientName = header.Value.PatientName,
            LaboratoryNumber = header.Value.LabId,
            PatientBarcode = $"LAB-{header.Value.LabId}",
            AgeSex = $"{header.Value.AgeYears}y / {header.Value.Gender}",
            ReferredBy = header.Value.ReferredBy,
            VisitDate = header.Value.VisitDate,
            RequestedAt = header.Value.VisitDate,
            DoctorSignatureLine = "طبيب المعمل",
            Results = lines
        };

        return new VisitReportPrintData(payload, lastPrintedAt, lastPrintedByUserId, enteredResultIds, cultureItemId);
    }

    private async Task<(string PatientName, string LabId, DateTime VisitDate, int AgeYears, Gender Gender, string ReferredBy)?> BuildHeaderAsync(
        int patientVisitId, CancellationToken ct)
    {
        var visitInfo = await (
                from visit in _context.PatientVisits.AsNoTracking()
                join patient in _context.Patients.AsNoTracking()
                    on visit.PatientId equals patient.Id
                where visit.Id == patientVisitId
                select new
                {
                    PatientName = patient.Name,
                    LabId = visit.LabId,
                    VisitDate = visit.VisitDate,
                    AgeYears = patient.Age.Years,
                    Gender = patient.Gender,
                    visit.DoctorId,
                    visit.ReferralEntityId
                })
            .SingleOrDefaultAsync(ct);
        if (visitInfo is null)
            return null;

        var referredBy = string.Empty;
        if (visitInfo.DoctorId is not null)
        {
            referredBy = await _context.Doctors.AsNoTracking()
                .Where(d => d.Id == visitInfo.DoctorId)
                .Select(d => d.Name)
                .FirstOrDefaultAsync(ct) ?? string.Empty;
        }
        else if (visitInfo.ReferralEntityId is not null)
        {
            referredBy = await _context.ReferralEntities.AsNoTracking()
                .Where(r => r.Id == visitInfo.ReferralEntityId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync(ct) ?? string.Empty;
        }

        return (visitInfo.PatientName, visitInfo.LabId, visitInfo.VisitDate,
            visitInfo.AgeYears, visitInfo.Gender, referredBy);
    }

    private async Task<(List<ClinicalResultLineDto>, DateTime?, int?, List<int>)> BuildIndividualAsync(
        int visitTestId, DateTime visitDate, CancellationToken ct)
    {
        // OQ-M4-5: excluded rows never reach the payload.
        var items = await _context.VisitTestResultItems.AsNoTracking()
            .Where(ri => ri.VisitTestId == visitTestId && !ri.IsDeleted && ri.IncludeInPrint)
            .OrderBy(ri => ri.DisplayOrder)
            .Select(ri => new { ri.Id, ri.ComponentName, ri.ComponentUnit })
            .ToListAsync(ct);

        var itemIds = items.Select(i => i.Id).ToList();
        var resultsById = await _context.TestResults.AsNoTracking()
            .Where(tr => itemIds.Contains(tr.VisitTestResultItemId) && !tr.IsDeleted)
            .ToDictionaryAsync(tr => tr.VisitTestResultItemId, ct);

        var lines = new List<ClinicalResultLineDto>();
        var enteredResultIds = new List<int>();
        DateTime? lastPrintedAt = null;
        int? lastPrintedByUserId = null;

        foreach (var item in items)
        {
            if (!resultsById.TryGetValue(item.Id, out var result))
                continue;
            enteredResultIds.Add(result.Id);
            lines.Add(new ClinicalResultLineDto(
                item.ComponentName,
                result.Value,
                string.IsNullOrEmpty(result.Unit) ? item.ComponentUnit : result.Unit,
                result.ReferenceRange,
                result.Status == ResultStatus.Normal ? string.Empty : result.Status.ToString()));

            if (result.PrintCount > 0 && result.PrintedAt is not null &&
                (lastPrintedAt is null || result.PrintedAt > lastPrintedAt))
            {
                lastPrintedAt = result.PrintedAt;
                lastPrintedByUserId = result.PrintedByUserId;
            }
        }

        return (lines, lastPrintedAt, lastPrintedByUserId, enteredResultIds);
    }

    private async Task<(List<ClinicalResultLineDto>, DateTime?, int?)> BuildConsolidatedAsync(
        int reportId, CancellationToken ct)
    {
        var dto = await _clinicalReportReader.GetConsolidatedAsync(reportId, ct);
        if (dto is null)
            return (new List<ClinicalResultLineDto>(), null, null);

        var lines = dto.Lines
            .Select(l => l.Flag == "SUBTITLE"
                ? new ClinicalResultLineDto(l.TestName, string.Empty, string.Empty, string.Empty, "SUBTITLE")
                : new ClinicalResultLineDto(l.TestName, l.Value, l.Unit, l.ReferenceRange))
            .ToList();

        var meta = await _context.ConsolidatedReports.AsNoTracking()
            .Where(r => r.Id == reportId)
            .Select(r => new { r.PrintedAt, r.PrintedByUserId })
            .FirstOrDefaultAsync(ct);

        return (lines, meta?.PrintedAt, meta?.PrintedByUserId);
    }

    private async Task<(List<ClinicalResultLineDto>, DateTime?, int?)> BuildBlankAsync(
        int reportId, CancellationToken ct)
    {
        var dto = await _blankReportReader.GetAsync(reportId, ct);
        if (dto is null)
            return (new List<ClinicalResultLineDto>(), null, null);

        var lines = dto.Rows
            .Select(row => new ClinicalResultLineDto(row.TestName, row.Result, row.Unit, row.ReferenceRange))
            .ToList();

        var meta = await _context.BlankReports.AsNoTracking()
            .Where(r => r.Id == reportId)
            .Select(r => new { r.PrintedAt, r.PrintedByUserId })
            .FirstOrDefaultAsync(ct);

        return (lines, meta?.PrintedAt, meta?.PrintedByUserId);
    }

    private async Task<(List<ClinicalResultLineDto>, int? CultureItemId, DateTime?, int?)> BuildCultureAsync(
        int visitTestId, CancellationToken ct)
    {
        var cultureLink = await (
                from item in _context.VisitTestResultItems.AsNoTracking()
                join culture in _context.Cultures.AsNoTracking()
                    on item.Id equals culture.VisitTestResultItemId
                where item.VisitTestId == visitTestId
                select new { ItemId = item.Id, CultureId = culture.Id })
            .FirstOrDefaultAsync(ct);
        if (cultureLink is null)
            return (new List<ClinicalResultLineDto>(), null, null, null);

        var dto = await _microbiologyReportReader.GetAsync(cultureLink.CultureId, ct);
        if (dto is null)
            return (new List<ClinicalResultLineDto>(), cultureLink.ItemId, null, null);

        var lines = new List<ClinicalResultLineDto>();
        foreach (var finding in dto.MicroscopicRows)
            lines.Add(new ClinicalResultLineDto(finding.RowKey, finding.Value, string.Empty, finding.ReferenceRange, "MICRO"));

        if (dto.ShowSensitivityInReport)
        {
            foreach (var block in dto.OrganismBlocks)
            {
                lines.Add(new ClinicalResultLineDto(block.OrganismLabel, string.Empty, string.Empty, string.Empty, "SUBTITLE"));
                foreach (var row in block.Rows)
                {
                    lines.Add(new ClinicalResultLineDto(
                        row.AntibioticName,
                        row.Level,
                        row.CommercialName,
                        dto.ShowReferenceInReport ? row.InhibitionZone : string.Empty));
                }
            }
        }

        var lastReceipt = await _context.CulturePrintReceipts.AsNoTracking()
            .Where(receipt => receipt.VisitTestResultItemId == cultureLink.ItemId)
            .OrderByDescending(receipt => receipt.PrintedAt)
            .Select(receipt => new { receipt.PrintedAt, receipt.PrintedByUserId })
            .FirstOrDefaultAsync(ct);

        return (lines, cultureLink.ItemId, lastReceipt?.PrintedAt, lastReceipt?.PrintedByUserId);
    }
}
