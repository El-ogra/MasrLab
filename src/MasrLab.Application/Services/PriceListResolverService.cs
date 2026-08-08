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
    private readonly IPriceListItemRepository _priceListItems;

    public PriceListResolverService(IPriceListItemRepository priceListItems)
    {
        _priceListItems = priceListItems ?? throw new ArgumentNullException(nameof(priceListItems));
    }

    /// <summary>
    /// يبحث عن سعر الاختبار بناءً على قائمة الأسعار المرجعية.
    /// INV: يجب تمرير priceListId صالح. غياب بند السعر للاختبار استثناء مجال،
    /// ولا يُعاد صفر أبدًا نتيجة «لا يوجد بند» — الصفر قيمة مشروعة فقط لبند موجود فعلاً.
    /// </summary>
    public async Task<decimal> ResolvePriceAsync(int testId, int priceListId, CancellationToken ct = default)
    {
        if (priceListId <= 0)
            throw new BusinessRuleViolationException("Price list ID must be greater than zero.");

        var item = await _priceListItems.GetByPriceListAndTestAsync(priceListId, testId, ct);

        if (item is null)
            throw new BusinessRuleViolationException(
                $"No price item found for test {testId} in price list {priceListId}.");

        return item.Price;
    }
}
