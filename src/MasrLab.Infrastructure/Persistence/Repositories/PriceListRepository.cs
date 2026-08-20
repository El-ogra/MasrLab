using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class PriceListRepository : GenericRepository<PriceList>, IPriceListRepository
{
    public PriceListRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<PriceList?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PriceLists
            .FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
    }

    public async Task<PriceList?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.PriceLists
            .AsNoTracking()
            .Include(p => p.PriceListItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PriceList?> GetLabToLabAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PriceLists
            .FirstOrDefaultAsync(p => p.IsLabToLab, cancellationToken);
    }
}
