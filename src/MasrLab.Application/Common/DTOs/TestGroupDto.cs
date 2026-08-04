namespace MasrLab.Application.Common.DTOs;

public record TestGroupDto
{
    public int Id { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public decimal GroupPrice { get; init; }
}
