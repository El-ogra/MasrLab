using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Common.DTOs;

public record GenderStatisticsDto
{
    public int PatientCount { get; init; }
    public int SampleCount { get; init; }
    public decimal TotalRevenue { get; init; }
}
