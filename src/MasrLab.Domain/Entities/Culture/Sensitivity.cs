using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Culture;

public class Sensitivity : BaseEntity
{
    public int CultureId { get; set; }
    public int AntibioticId { get; set; }
    public SensitivityLevel SensitivityLevel { get; set; }
}
