namespace MasrLab.Domain.Services;

public interface IPriceListResolverService
{
    decimal ResolvePrice(int testId, int? referralEntityId, int? priceListId);
    Task<decimal> ResolvePriceAsync(int testId, int? referralEntityId, int? priceListId, CancellationToken ct = default);
}
