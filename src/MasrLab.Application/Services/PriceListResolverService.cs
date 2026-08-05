using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// محلل أسعار قوائم الأسعار — يبحث عن السعر المناسب لاختبار معين.
/// </summary>
public class PriceListResolverService : IPriceListResolverService
{
    private readonly IRepository<PriceListItem> _priceListItems;

    public PriceListResolverService(IRepository<PriceListItem> priceListItems)
    {
        _priceListItems = priceListItems ?? throw new ArgumentNullException(nameof(priceListItems));
    }

    /// <summary>
    /// يبحث عن سعر الاختبار بناءً على قائمة الأسعار المرجعية.
    /// INV: يجب تمرير priceListId صالح. يُرجع 0 إذا لم يُوجد سعر للاختبار في القائمة.
    /// </summary>
    public async Task<decimal> ResolvePriceAsync(int testId, int priceListId, CancellationToken ct = default)
    {
        if (priceListId <= 0)
            throw new BusinessRuleViolationException("Price list ID must be greater than zero.");

        var allItems = await _priceListItems.GetAllAsync(ct);
        var item = allItems.FirstOrDefault(i =>
            i.PriceListId == priceListId && i.TestId == testId);

        return item?.Price ?? 0;
    }
}
