using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class VisitTestResultItemRepository : GenericRepository<VisitTestResultItem>, IVisitTestResultItemRepository
{
    public VisitTestResultItemRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<VisitTestResultItem?> GetByVisitTestIdAndComponentIdAsync(int visitTestId, int sourceTestComponentId, CancellationToken cancellationToken = default)
    {
        return await _context.VisitTestResultItems
            .AsNoTracking()
            .FirstOrDefaultAsync(vri => vri.VisitTestId == visitTestId && vri.SourceTestComponentId == sourceTestComponentId, cancellationToken);
    }

    public async Task<IReadOnlyList<VisitTestResultItem>> GetByVisitTestIdAsync(int visitTestId, CancellationToken cancellationToken = default)
    {
        return await _context.VisitTestResultItems
            .AsNoTracking()
            .Where(vri => vri.VisitTestId == visitTestId)
            .ToListAsync(cancellationToken);
    }
}
