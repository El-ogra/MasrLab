using MasrLab.Domain.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class StatisticsRepository : IStatisticsRepository
{
    private readonly MasrLabDbContext _context;

    public StatisticsRepository(MasrLabDbContext context)
    {
        _context = context;
    }

    public async Task<StatisticsDto> GetGenderStatisticsAsync(DateTime start, DateTime end)
    {
        return await Task.FromResult(new StatisticsDto());
    }

    public async Task<StatisticsDto> GetMonthlyStatisticsAsync(DateTime start, DateTime end)
    {
        return await Task.FromResult(new StatisticsDto());
    }

    public async Task<StatisticsDto> GetPatientCountByPeriodAsync(DateTime start, DateTime end)
    {
        return await Task.FromResult(new StatisticsDto());
    }

    public async Task<StatisticsDto> GetSampleCountByYearAsync(int year)
    {
        return await Task.FromResult(new StatisticsDto());
    }

    public async Task<StatisticsDto> GetTestDemandRateAsync(DateTime start, DateTime end)
    {
        return await Task.FromResult(new StatisticsDto());
    }
}
