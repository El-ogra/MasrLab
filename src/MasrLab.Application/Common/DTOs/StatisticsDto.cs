namespace MasrLab.Application.Common.DTOs;

public record StatisticsDto
{
    public int PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public int VisitCount { get; init; }
    public int SampleCount { get; init; }
    public decimal TotalRevenue { get; init; }
}
