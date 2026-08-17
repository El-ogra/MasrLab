using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record ReferenceValueDto
{
    public int Id { get; init; }
    public int TestId { get; init; }
    public int TestComponentId { get; init; }
    public ReferenceValueGender Gender { get; init; }
    public int AgeMin { get; init; }
    public int AgeMax { get; init; }
    public AgeUnit AgeUnit { get; init; }
    public string NormalRange { get; init; } = string.Empty;
    public decimal? LowLimit { get; init; }
    public decimal? HighLimit { get; init; }
    public string? TestUnit { get; init; }
    public string? LowFlag { get; init; }
    public string? HighFlag { get; init; }
    public bool ForPregnantOnly { get; init; }
    public string? HighComment { get; init; }
    public string? LowComment { get; init; }
}
