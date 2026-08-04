using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record GenderStatisticsDto
{
    public Gender Gender { get; init; }
    public int PatientCount { get; init; }
    public int SampleCount { get; init; }
    public decimal TotalRevenue { get; init; }
}
