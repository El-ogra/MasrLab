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

    public async Task<GenderStatisticsDto> GetGenderStatisticsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var patientCount = await _context.Patients
            .CountAsync(p => p.CreatedAt >= start && p.CreatedAt <= end, cancellationToken);

        var sampleCount = await _context.VisitTests
            .Where(vt => vt.CreatedAt >= start && vt.CreatedAt <= end)
            .CountAsync(cancellationToken);

        var totalRevenue = await _context.Receipts
            .Where(r => r.IssueDate >= start && r.IssueDate <= end)
            .SumAsync(r => r.Total, cancellationToken);

        return new GenderStatisticsDto
        {
            PatientCount = patientCount,
            SampleCount = sampleCount,
            TotalRevenue = totalRevenue
        };
    }

    public async Task<MonthlyStatisticsDto> GetMonthlyStatisticsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var patientCount = await _context.Patients
            .CountAsync(p => p.CreatedAt >= start && p.CreatedAt <= end, cancellationToken);

        var sampleCount = await _context.VisitTests
            .Where(vt => vt.CreatedAt >= start && vt.CreatedAt <= end)
            .CountAsync(cancellationToken);

        var totalRevenue = await _context.Receipts
            .Where(r => r.IssueDate >= start && r.IssueDate <= end)
            .SumAsync(r => r.Total, cancellationToken);

        return new MonthlyStatisticsDto
        {
            Year = start.Year,
            Month = start.Month,
            PatientCount = patientCount,
            SampleCount = sampleCount,
            TotalRevenue = totalRevenue
        };
    }

    public async Task<PatientCountByPeriodDto> GetPatientCountByPeriodAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var allPatientsInRange = await _context.Patients
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .ToListAsync(cancellationToken);

        var returningPatientIds = await _context.PatientVisits
            .Where(v => v.VisitDate < start)
            .Select(v => v.PatientId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var returningCount = allPatientsInRange.Count(p => returningPatientIds.Contains(p.Id));
        var newCount = allPatientsInRange.Count - returningCount;

        return new PatientCountByPeriodDto
        {
            PeriodStart = start,
            PeriodEnd = end,
            NewPatients = newCount,
            ReturningPatients = returningCount,
            TotalPatients = allPatientsInRange.Count
        };
    }

    public async Task<SampleCountByYearDto> GetSampleCountByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31, 23, 59, 59);

        var totalSamples = await _context.VisitTests
            .Where(vt => vt.CreatedAt >= start && vt.CreatedAt <= end)
            .CountAsync(cancellationToken);

        var collectedSamples = await _context.Samples
            .Where(s => s.CollectedAt >= start && s.CollectedAt <= end && s.CollectionStatus == Domain.Common.Enums.SampleStatus.Collected)
            .CountAsync(cancellationToken);

        return new SampleCountByYearDto
        {
            Year = year,
            TotalSamples = totalSamples,
            CollectedSamples = collectedSamples,
            PendingSamples = totalSamples - collectedSamples
        };
    }

    public async Task<TestDemandRateDto> GetTestDemandRateAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var totalVisits = await _context.VisitTests
            .Where(vt => vt.CreatedAt >= start && vt.CreatedAt <= end)
            .CountAsync(cancellationToken);

        var topTest = await _context.VisitTests
            .Where(vt => vt.CreatedAt >= start && vt.CreatedAt <= end)
            .GroupBy(vt => vt.TestId)
            .Select(g => new { TestId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync(cancellationToken);

        if (topTest is null)
        {
            return new TestDemandRateDto
            {
                TestId = 0,
                TestName = string.Empty,
                RequestCount = 0,
                DemandPercentage = 0
            };
        }

        var testName = await _context.Tests
            .Where(t => t.Id == topTest.TestId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return new TestDemandRateDto
        {
            TestId = topTest.TestId,
            TestName = testName,
            RequestCount = topTest.Count,
            DemandPercentage = totalVisits > 0 ? (decimal)topTest.Count / totalVisits * 100 : 0
        };
    }
}
