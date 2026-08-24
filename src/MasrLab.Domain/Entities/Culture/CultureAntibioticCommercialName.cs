using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Culture;

public class CultureAntibioticCommercialName : BaseEntity
{
    public int CultureAntibioticId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Print { get; set; }

    public CultureAntibiotic? CultureAntibiotic { get; set; }
}
