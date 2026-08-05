using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ITestGroupItemRepository : IRepository<TestGroupItem>
{
    Task<IReadOnlyList<TestGroupItem>> GetByTestGroupIdAsync(int testGroupId, CancellationToken cancellationToken = default);
}
