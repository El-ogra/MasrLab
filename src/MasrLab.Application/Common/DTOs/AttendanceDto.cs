namespace MasrLab.Application.Common.DTOs;

public record AttendanceDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public DateTime LoginTime { get; init; }
    public DateTime? LogoutTime { get; init; }
    public TimeSpan? Overtime { get; init; }
    public TimeSpan? Delays { get; init; }
    public string? BreakPeriods { get; init; }
}
