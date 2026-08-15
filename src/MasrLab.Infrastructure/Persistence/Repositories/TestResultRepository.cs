using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class TestResultRepository : GenericRepository<TestResult>, ITestResultRepository
{
    public TestResultRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TestResult>> GetByVisitTestResultItemIdAsync(int visitTestResultItemId, CancellationToken cancellationToken = default)
    {
        return await _context.TestResults
            .Where(tr => tr.VisitTestResultItemId == visitTestResultItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TestResult>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var visitIds = await _context.PatientVisits
            .Where(v => v.PatientId == patientId)
            .Select(v => v.Id)
            .ToListAsync(cancellationToken);

        var resultItemIds = await _context.VisitTestResultItems
            .Where(vri => visitIds.Contains(vri.VisitTestId))
            .Select(vri => vri.Id)
            .ToListAsync(cancellationToken);

        return await _context.TestResults
            .Where(tr => resultItemIds.Contains(tr.VisitTestResultItemId))
            .ToListAsync(cancellationToken);
    }
}
