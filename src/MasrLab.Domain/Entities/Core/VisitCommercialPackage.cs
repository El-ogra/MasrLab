using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class VisitCommercialPackage : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int CommercialPackageId { get; set; }
    public string PackageNameSnapshot { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
