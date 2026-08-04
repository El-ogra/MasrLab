namespace MasrLab.Domain.Common.DTOs;

public record TestDemandRateDto
{
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public decimal DemandPercentage { get; init; }
}
