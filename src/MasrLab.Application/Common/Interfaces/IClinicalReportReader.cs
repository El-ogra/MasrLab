namespace MasrLab.Application.Common.Interfaces;

using MasrLab.Application.Features.ResultsEntry.Queries.GetConsolidatedReport;
using MasrLab.Application.Features.ResultsEntry.Queries.GetBlankReport;

// M4 rendering seam: rebuilds persisted clinical report payloads (Slices 9/8).
public interface IClinicalReportReader
{
    Task<ConsolidatedReportDto?> GetConsolidatedAsync(int consolidatedReportId, CancellationToken cancellationToken = default);
}
