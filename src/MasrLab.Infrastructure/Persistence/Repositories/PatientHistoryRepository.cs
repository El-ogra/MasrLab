using MasrLab.Domain.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Views;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class PatientHistoryRepository : IPatientHistoryRepository
{
    private readonly MasrLabDbContext _context;

    public PatientHistoryRepository(MasrLabDbContext context)
        => _context = context;

    public async Task<IReadOnlyList<PatientHistoryEntry>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var rows = await _context.PatientHistoryViews
            .Where(v => v.PatientId == patientId)
            .OrderBy(v => v.TestId)
            .ThenBy(v => v.CurrentVisitDate)
            .ToListAsync(cancellationToken);

        return rows.Select(MapToEntry).ToList();
    }

    public async Task<IReadOnlyList<PatientHistoryEntry>> GetByPatientAndTestAsync(int patientId, int testId, CancellationToken cancellationToken = default)
    {
        var rows = await _context.PatientHistoryViews
            .Where(v => v.PatientId == patientId && v.TestId == testId)
            .OrderBy(v => v.CurrentVisitDate)
            .ToListAsync(cancellationToken);

        return rows.Select(MapToEntry).ToList();
    }

    private static PatientHistoryEntry MapToEntry(PatientHistoryView v) => new()
    {
        PatientId = v.PatientId,
        LabId = v.LabId,
        TestId = v.TestId,
        TestName = v.TestName,
        TestReportName = v.TestReportName,
        PreviousValue = v.PreviousValue,
        PreviousUnit = v.PreviousUnit,
        PreviousReferenceRange = v.PreviousReferenceRange,
        PreviousStatus = v.PreviousStatus ?? string.Empty,
        PreviousVisitDate = v.PreviousVisitDate,
        CurrentValue = v.CurrentValue,
        CurrentUnit = v.CurrentUnit,
        CurrentReferenceRange = v.CurrentReferenceRange,
        CurrentStatus = v.CurrentStatus ?? string.Empty,
        CurrentVisitDate = v.CurrentVisitDate,
        ComparisonFlag = v.CurrentValue != v.PreviousValue
            || v.CurrentUnit != v.PreviousUnit
            || v.CurrentReferenceRange != v.PreviousReferenceRange
    };
}
