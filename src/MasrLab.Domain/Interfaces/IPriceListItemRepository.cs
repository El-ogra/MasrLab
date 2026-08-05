using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Domain.Interfaces;

public interface IPriceListItemRepository : IRepository<PriceListItem>
{
    Task<PriceListItem?> GetByPriceListAndTestAsync(int priceListId, int testId, CancellationToken cancellationToken = default);
}
