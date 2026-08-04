namespace MasrLab.Application.Common.DTOs;

public record TestDemandRateDto
{
    public TestDemandRateDto() { }
    public TestDemandRateDto(Domain.Common.DTOs.TestDemandRateDto source)
    {
        TestId = source.TestId;
        TestName = source.TestName;
        RequestCount = source.RequestCount;
        DemandPercentage = source.DemandPercentage;
    }
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public decimal DemandPercentage { get; init; }
}
