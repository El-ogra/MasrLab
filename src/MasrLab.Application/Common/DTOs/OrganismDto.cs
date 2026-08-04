namespace MasrLab.Application.Common.DTOs;

public record OrganismDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
