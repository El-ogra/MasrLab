namespace MasrLab.Application.Common.DTOs;

public record SampleCountByYearDto
{
    public SampleCountByYearDto() { }
    public SampleCountByYearDto(Domain.Common.DTOs.SampleCountByYearDto source)
    {
        Year = source.Year;
        TotalSamples = source.TotalSamples;
        CollectedSamples = source.CollectedSamples;
        PendingSamples = source.PendingSamples;
    }
    public int Year { get; init; }
    public int TotalSamples { get; init; }
    public int CollectedSamples { get; init; }
    public int PendingSamples { get; init; }
}
