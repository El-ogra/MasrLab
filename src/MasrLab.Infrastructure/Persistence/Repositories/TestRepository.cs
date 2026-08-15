using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class TestRepository : GenericRepository<Test>, ITestRepository
{
    public TestRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Test>> GetAllWithComponentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tests
            .Include(t => t.TestComponents.Where(c => !c.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public async Task<Test?> GetByIdWithComponentsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tests
            .Include(t => t.TestComponents.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}
