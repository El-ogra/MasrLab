using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Domain.Interfaces;

public interface IStatisticsRepository
{
    Task<GenderStatisticsDto> GetGenderStatisticsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<MonthlyStatisticsDto> GetMonthlyStatisticsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<PatientCountByPeriodDto> GetPatientCountByPeriodAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<SampleCountByYearDto> GetSampleCountByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<TestDemandRateDto> GetTestDemandRateAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
