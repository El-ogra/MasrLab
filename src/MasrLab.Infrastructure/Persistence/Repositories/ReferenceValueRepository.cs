using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class ReferenceValueRepository : GenericRepository<ReferenceValue>, IReferenceValueRepository
{
    public ReferenceValueRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ReferenceValue>> GetByTestIdAsync(int testId, CancellationToken cancellationToken = default)
    {
        return await _context.ReferenceValues
            .AsNoTracking()
            .Where(rv => rv.TestId == testId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceValue>> GetByTestComponentIdAsync(int testComponentId, CancellationToken cancellationToken = default)
    {
        return await _context.ReferenceValues
            .AsNoTracking()
            .Where(rv => rv.TestComponentId == testComponentId)
            .ToListAsync(cancellationToken);
    }
}
