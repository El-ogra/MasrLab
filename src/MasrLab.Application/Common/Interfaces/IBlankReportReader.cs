namespace MasrLab.Application.Common.Interfaces;

using MasrLab.Application.Features.ResultsEntry.Queries.GetBlankReport;

// OQ-M4-8: persisted blank reports are re-printable — the reader rebuilds the payload.
public interface IBlankReportReader
{
    Task<BlankReportDto?> GetAsync(int blankReportId, CancellationToken cancellationToken = default);
}
