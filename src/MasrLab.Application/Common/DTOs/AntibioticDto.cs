namespace MasrLab.Application.Common.DTOs;

public record AntibioticDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ScientificName { get; init; } = string.Empty;
}
