namespace MasrLab.Domain.Services;

public interface IPriceListResolverService
{
    Task<decimal> ResolvePriceAsync(int testId, int priceListId, CancellationToken ct = default);
}
