namespace MasrLab.Application.Common.Interfaces;

using MasrLab.Application.Features.ResultsEntry.Queries.GetReprintWarning;

// OQ-M4-7: last print user + timestamp for a visit test, when it has been printed before.
public interface IReprintWarningReader
{
    Task<ReprintWarningDto> GetAsync(int visitTestId, CancellationToken cancellationToken = default);
}
