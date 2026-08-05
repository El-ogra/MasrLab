using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class TestResultRepository : GenericRepository<TestResult>, ITestResultRepository
{
    public TestResultRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TestResult>> GetByVisitTestIdAsync(int visitTestId, CancellationToken cancellationToken = default)
    {
        return await _context.TestResults
            .Where(tr => tr.VisitTestId == visitTestId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TestResult>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var visitIds = await _context.PatientVisits
            .Where(v => v.PatientId == patientId)
            .Select(v => v.Id)
            .ToListAsync(cancellationToken);

        var visitTestIds = await _context.VisitTests
            .Where(vt => visitIds.Contains(vt.PatientVisitId))
            .Select(vt => vt.Id)
            .ToListAsync(cancellationToken);

        return await _context.TestResults
            .Where(tr => visitTestIds.Contains(tr.VisitTestId))
            .ToListAsync(cancellationToken);
    }
}
