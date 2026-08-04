using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Domain.Interfaces;

public interface IStatisticsRepository
{
    Task<GenderStatisticsDto> GetGenderStatisticsAsync(DateTime start, DateTime end);
    Task<MonthlyStatisticsDto> GetMonthlyStatisticsAsync(DateTime start, DateTime end);
    Task<PatientCountByPeriodDto> GetPatientCountByPeriodAsync(DateTime start, DateTime end);
    Task<SampleCountByYearDto> GetSampleCountByYearAsync(int year);
    Task<TestDemandRateDto> GetTestDemandRateAsync(DateTime start, DateTime end);
}
