using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ITestGroupRepository : IRepository<TestGroup>
{
    Task<IReadOnlyList<TestGroup>> GetAllWithItemsAsync(CancellationToken cancellationToken = default);
    Task<TestGroup?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
}
