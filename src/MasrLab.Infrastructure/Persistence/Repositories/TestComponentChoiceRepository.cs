using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class TestComponentChoiceRepository : GenericRepository<TestComponentChoice>, ITestComponentChoiceRepository
{
    public TestComponentChoiceRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TestComponentChoice>> GetByTestComponentIdAsync(int testComponentId, CancellationToken ct = default)
    {
        return await _context.TestComponentChoices
            .AsNoTracking()
            .Where(c => c.TestComponentId == testComponentId && !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }
}
