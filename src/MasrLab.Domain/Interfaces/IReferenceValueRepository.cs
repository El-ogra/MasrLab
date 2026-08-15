using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface IReferenceValueRepository : IRepository<ReferenceValue>
{
    Task<IReadOnlyList<ReferenceValue>> GetByTestIdAsync(int testId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReferenceValue>> GetByTestComponentIdAsync(int testComponentId, CancellationToken cancellationToken = default);
}
