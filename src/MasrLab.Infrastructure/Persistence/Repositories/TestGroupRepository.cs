using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class TestGroupRepository : GenericRepository<TestGroup>, ITestGroupRepository
{
    public TestGroupRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TestGroup>> GetAllWithItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TestGroups
            .AsNoTracking()
            .Include(g => g.TestGroupItems)
            .ToListAsync(cancellationToken);
    }

    public async Task<TestGroup?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TestGroups
            .AsNoTracking()
            .Include(g => g.TestGroupItems)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken = default)
    {
        return await _context.TestGroups
            .AnyAsync(g => g.GroupName == name && !g.IsDeleted && (!excludeId.HasValue || g.Id != excludeId.Value), cancellationToken);
    }
}
