using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class CommercialPackage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<CommercialPackageItem> Items { get; set; } = new List<CommercialPackageItem>();
    public ICollection<CommercialPackagePrice> Prices { get; set; } = new List<CommercialPackagePrice>();
}
