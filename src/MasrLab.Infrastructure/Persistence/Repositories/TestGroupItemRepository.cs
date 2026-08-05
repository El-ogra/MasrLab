using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class TestGroupItemRepository : GenericRepository<TestGroupItem>, ITestGroupItemRepository
{
    public TestGroupItemRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TestGroupItem>> GetByTestGroupIdAsync(int testGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.TestGroupItems
            .AsNoTracking()
            .Where(i => i.TestGroupId == testGroupId)
            .ToListAsync(cancellationToken);
    }
}
