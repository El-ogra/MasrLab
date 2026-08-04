namespace MasrLab.Application.Common.DTOs;

public record MonthlyStatisticsDto
{
    public MonthlyStatisticsDto() { }
    public MonthlyStatisticsDto(Domain.Common.DTOs.MonthlyStatisticsDto source)
    {
        Year = source.Year;
        Month = source.Month;
        PatientCount = source.PatientCount;
        SampleCount = source.SampleCount;
        TotalRevenue = source.TotalRevenue;
    }
    public int Year { get; init; }
    public int Month { get; init; }
    public int PatientCount { get; init; }
    public int SampleCount { get; init; }
    public decimal TotalRevenue { get; init; }
}
