using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Core;

public class ReferenceValue : BaseEntity
{
    public int TestId { get; set; }
    public int? TestComponentId { get; set; }
    public ReferenceValueGender Gender { get; set; }
    public int AgeMin { get; set; }
    public int AgeMax { get; set; }
    public AgeUnit AgeUnit { get; set; }
    public string NormalRange { get; set; } = string.Empty;
    public decimal? LowLimit { get; set; }
    public decimal? HighLimit { get; set; }
    public string? TestUnit { get; set; }
    public string? LowFlag { get; set; }
    public string? HighFlag { get; set; }
    public bool ForPregnantOnly { get; set; }
    public string? HighComment { get; set; }
    public string? LowComment { get; set; }
}
