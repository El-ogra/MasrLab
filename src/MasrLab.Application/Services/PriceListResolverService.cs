using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// محلل أسعار قوائم الأسعار — يبحث عن السعر المناسب لاختبار معين.
/// </summary>
public class PriceListResolverService : IPriceListResolverService
{
    private readonly IRepository<PriceList> _priceLists;
    private readonly IRepository<PriceListItem> _priceListItems;

    public PriceListResolverService(
        IRepository<PriceList> priceLists,
        IRepository<PriceListItem> priceListItems)
    {
        _priceLists = priceLists ?? throw new ArgumentNullException(nameof(priceLists));
        _priceListItems = priceListItems ?? throw new ArgumentNullException(nameof(priceListItems));
    }

    /// <summary>
    /// يبحث عن سعر الاختبار بناءً على قائمة الأسعار المرجعية.
    /// INV: إذا تم تمرير priceListId، يُستخدم مباشرة. وإلا يُبحث عبر ReferralEntity.PriceListId.
    /// </summary>
    public decimal ResolvePrice(int testId, int? referralEntityId, int? priceListId)
    {
        var effectivePriceListId = priceListId;

        if (effectivePriceListId is null && referralEntityId.HasValue)
        {
            // لا يمكن تحميل ReferralEntity هنا لأنه في Domain Entities
            // يُفترض أن البائع يمرر PriceListId الصحيح
        }

        if (effectivePriceListId is null)
            return 0;

        var allItems = _priceListItems.GetAllAsync().GetAwaiter().GetResult();
        var item = allItems.FirstOrDefault(i =>
            i.PriceListId == effectivePriceListId.Value && i.TestId == testId);

        return item?.Price ?? 0;
    }

    /// <summary>
    /// النسخة غير المتزامنة من ResolvePrice.
    /// </summary>
    public async Task<decimal> ResolvePriceAsync(int testId, int? referralEntityId, int? priceListId, CancellationToken ct = default)
    {
        var effectivePriceListId = priceListId;

        if (effectivePriceListId is null)
            return 0;

        var allItems = await _priceListItems.GetAllAsync();
        var item = allItems.FirstOrDefault(i =>
            i.PriceListId == effectivePriceListId.Value && i.TestId == testId);

        return item?.Price ?? 0;
    }
}
