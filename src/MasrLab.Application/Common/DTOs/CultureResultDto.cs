using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record CultureResultDto
{
    public int Id { get; init; }
    public string SampleType { get; init; } = string.Empty;
    public string? OrganismA { get; init; }
    public string? OrganismB { get; init; }
    public string? OrganismC { get; init; }
    public string CultureCondition { get; init; } = string.Empty;
    public int ColonyCount { get; init; }
}
