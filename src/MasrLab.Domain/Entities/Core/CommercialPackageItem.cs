using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class CommercialPackageItem : BaseEntity
{
    public int CommercialPackageId { get; set; }
    public int TestId { get; set; }
    public int DisplayOrder { get; set; }
}
