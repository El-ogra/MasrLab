using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Domain.Interfaces;

public interface IPriceListRepository : IRepository<PriceList>
{
    Task<PriceList?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<PriceList?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<PriceList?> GetLabToLabAsync(CancellationToken cancellationToken = default);
}
