namespace MasrLab.Application.Common.DTOs;

public record ExternalLabDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? ContactPerson { get; init; }
}
