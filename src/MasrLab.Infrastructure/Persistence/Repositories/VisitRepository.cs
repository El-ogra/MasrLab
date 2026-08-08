using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class VisitRepository : GenericRepository<PatientVisit>, IVisitRepository
{
    public VisitRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<VisitTest?> GetVisitTestAsync(int visitTestId, CancellationToken cancellationToken = default)
    {
        return await _context.VisitTests
            .AsNoTracking()
            .FirstOrDefaultAsync(vt => vt.Id == visitTestId, cancellationToken);
    }

    public async Task<IReadOnlyList<PatientVisit>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .Where(v => v.PatientId == patientId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PatientVisit>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .Where(v => v.VisitDate >= start && v.VisitDate <= end)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PatientVisit>> GetByDateRangeWithTestsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .AsNoTracking()
            .Where(v => v.VisitDate >= start && v.VisitDate <= end)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.TestResult)
            .Include(v => v.Samples)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PatientVisit>> GetPendingVisitsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .Where(v => v.Status == VisitStatus.Registered)
            .ToListAsync(cancellationToken);
    }
}
