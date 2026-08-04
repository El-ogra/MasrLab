namespace MasrLab.Domain.Common.DTOs;

public record SampleCountByYearDto
{
    public int Year { get; init; }
    public int TotalSamples { get; init; }
    public int CollectedSamples { get; init; }
    public int PendingSamples { get; init; }
}
