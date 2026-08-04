namespace MasrLab.Application.Common.DTOs;

public record MonthlyStatisticsDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int PatientCount { get; init; }
    public int SampleCount { get; init; }
    public decimal TotalRevenue { get; init; }
}
