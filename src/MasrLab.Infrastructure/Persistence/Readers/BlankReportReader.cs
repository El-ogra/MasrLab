using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.ResultsEntry.Queries.GetBlankReport;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

public sealed class BlankReportReader : IBlankReportReader
{
    private readonly MasrLabDbContext _context;

    public BlankReportReader(MasrLabDbContext context) => _context = context;

    public async Task<BlankReportDto?> GetAsync(int blankReportId, CancellationToken cancellationToken = default)
    {
        var report = await _context.BlankReports.AsNoTracking()
            .Where(r => r.Id == blankReportId)
            .Select(r => new
            {
                r.Id,
                r.PatientVisitId,
                r.ReportTitle,
                r.Comment,
                r.PaginationNote,
                r.PrintCount,
                Rows = r.Rows
                    .OrderBy(row => row.DisplayOrder)
                    .Select(row => new { row.Id, row.TestName, row.Result, row.Unit, row.Flag, row.ReferenceRange, row.DisplayOrder })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (report is null)
            return null;

        // Patient header (M4-BR-09) for the print payload.
        var visitInfo = await (
                from visit in _context.PatientVisits.AsNoTracking()
                join patient in _context.Patients.AsNoTracking()
                    on visit.PatientId equals patient.Id
                where visit.Id == report.PatientVisitId
                select new { patient.Name, visit.LabId, visit.VisitDate })
            .SingleOrDefaultAsync(cancellationToken);
        if (visitInfo is null)
            return null;

        return new BlankReportDto(
            report.Id,
            report.PatientVisitId,
            visitInfo.Name,
            visitInfo.LabId,
            visitInfo.VisitDate,
            report.ReportTitle,
            report.Comment,
            report.PaginationNote,
            report.PrintCount,
            report.Rows
                .Select(row => new BlankReportRowDto(row.Id, row.TestName, row.Result, row.Unit, row.Flag, row.ReferenceRange, row.DisplayOrder))
                .ToList());
    }
}
