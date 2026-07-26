using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Settings;

public class PriceListItem : BaseEntity
{
    public int PriceListId { get; set; }
    public int TestId { get; set; }
    public decimal Price { get; set; }
}
