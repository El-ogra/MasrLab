namespace MasrLab.Application.Common.DTOs;

public record PatientCountByPeriodDto
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int NewPatients { get; init; }
    public int ReturningPatients { get; init; }
    public int TotalPatients { get; init; }
}
