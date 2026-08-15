using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class CommercialPackagePrice : BaseEntity
{
    public int CommercialPackageId { get; set; }
    public int PriceListId { get; set; }
    public decimal Price { get; set; }
}
