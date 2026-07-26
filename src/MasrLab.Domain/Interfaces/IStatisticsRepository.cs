using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Domain.Interfaces;

public interface IStatisticsRepository
{
    Task<StatisticsDto> GetGenderStatisticsAsync(DateTime start, DateTime end);
    Task<StatisticsDto> GetMonthlyStatisticsAsync(DateTime start, DateTime end);
    Task<StatisticsDto> GetPatientCountByPeriodAsync(DateTime start, DateTime end);
    Task<StatisticsDto> GetSampleCountByYearAsync(int year);
    Task<StatisticsDto> GetTestDemandRateAsync(DateTime start, DateTime end);
}
