namespace MasrLab.Application.Common.DTOs;

public record PatientCountByPeriodDto
{
    public PatientCountByPeriodDto() { }
    public PatientCountByPeriodDto(Domain.Common.DTOs.PatientCountByPeriodDto source)
    {
        PeriodStart = source.PeriodStart;
        PeriodEnd = source.PeriodEnd;
        NewPatients = source.NewPatients;
        ReturningPatients = source.ReturningPatients;
        TotalPatients = source.TotalPatients;
    }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int NewPatients { get; init; }
    public int ReturningPatients { get; init; }
    public int TotalPatients { get; init; }
}
