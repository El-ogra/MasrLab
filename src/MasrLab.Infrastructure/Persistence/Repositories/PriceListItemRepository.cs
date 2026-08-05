using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class PriceListItemRepository : GenericRepository<PriceListItem>, IPriceListItemRepository
{
    public PriceListItemRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<PriceListItem?> GetByPriceListAndTestAsync(int priceListId, int testId, CancellationToken cancellationToken = default)
    {
        return await _context.PriceListItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.PriceListId == priceListId && i.TestId == testId, cancellationToken);
    }
}
