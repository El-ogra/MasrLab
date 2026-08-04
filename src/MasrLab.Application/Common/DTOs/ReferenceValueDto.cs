using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record ReferenceValueDto
{
    public int Id { get; init; }
    public int TestId { get; init; }
    public ReferenceValueGender Gender { get; init; }
    public int AgeMin { get; init; }
    public int AgeMax { get; init; }
    public AgeUnit AgeUnit { get; init; }
    public string NormalRange { get; init; } = string.Empty;
    public string? HighComment { get; init; }
    public string? LowComment { get; init; }
}
