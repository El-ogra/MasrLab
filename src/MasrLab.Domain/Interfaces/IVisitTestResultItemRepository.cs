using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface IVisitTestResultItemRepository : IRepository<VisitTestResultItem>
{
    Task<VisitTestResultItem?> GetByVisitTestIdAndComponentIdAsync(int visitTestId, int sourceTestComponentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VisitTestResultItem>> GetByVisitTestIdAsync(int visitTestId, CancellationToken cancellationToken = default);
}
