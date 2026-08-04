namespace MasrLab.Application.Common.DTOs;

public record GenderStatisticsDto
{
    public GenderStatisticsDto() { }
    public GenderStatisticsDto(Domain.Common.DTOs.GenderStatisticsDto source)
    {
        PatientCount = source.PatientCount;
        SampleCount = source.SampleCount;
        TotalRevenue = source.TotalRevenue;
    }
    public int PatientCount { get; init; }
    public int SampleCount { get; init; }
    public decimal TotalRevenue { get; init; }
}
