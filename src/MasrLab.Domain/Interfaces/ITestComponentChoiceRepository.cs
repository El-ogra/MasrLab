using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ITestComponentChoiceRepository : IRepository<TestComponentChoice>
{
    Task<IReadOnlyList<TestComponentChoice>> GetByTestComponentIdAsync(int testComponentId, CancellationToken ct = default);
}
