using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Core;

public class ReferenceValue : BaseEntity
{
    public int TestId { get; set; }
    public ReferenceValueGender Gender { get; set; }
    public int AgeMin { get; set; }
    public int AgeMax { get; set; }
    public AgeUnit AgeUnit { get; set; }
    public string NormalRange { get; set; } = string.Empty;
    public string? HighComment { get; set; }
    public string? LowComment { get; set; }
}
