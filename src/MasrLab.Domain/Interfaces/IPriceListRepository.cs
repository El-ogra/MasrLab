using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Domain.Interfaces;

public interface IPriceListRepository : IRepository<PriceList>
{
    Task<PriceList?> GetDefaultAsync(CancellationToken cancellationToken = default);
}
