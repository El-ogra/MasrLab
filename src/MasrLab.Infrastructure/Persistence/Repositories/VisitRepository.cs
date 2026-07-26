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

    public async Task<IReadOnlyList<PatientVisit>> GetByPatientIdAsync(int patientId)
    {
        return await _context.PatientVisits
            .Where(v => v.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PatientVisit>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.PatientVisits
            .Where(v => v.VisitDate >= start && v.VisitDate <= end)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PatientVisit>> GetPendingVisitsAsync()
    {
        return await _context.PatientVisits
            .Where(v => v.Status == VisitStatus.Registered)
            .ToListAsync();
    }
}
