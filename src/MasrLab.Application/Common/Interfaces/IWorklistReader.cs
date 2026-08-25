namespace MasrLab.Application.Common.Interfaces;

using MasrLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;
using MasrLab.Domain.Common.Enums;

public interface IWorklistReader
{
    Task<IReadOnlyList<WorklistPatientDto>> GetAsync(
        DateTime date,
        AccountType? category,
        CancellationToken cancellationToken = default);
}
