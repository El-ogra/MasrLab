namespace MasrLab.Application.Common.DTOs;

public record UserDto
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public bool IsAdmin { get; init; }
    public bool IsActive { get; init; }
}
