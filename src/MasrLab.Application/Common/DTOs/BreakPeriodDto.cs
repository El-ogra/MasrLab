namespace MasrLab.Application.Common.DTOs;

public record BreakPeriodDto
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public TimeSpan Duration { get; init; }
}
