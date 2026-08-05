using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Patient>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .Where(p => p.Name.Contains(name))
            .ToListAsync(cancellationToken);
    }

    public async Task<Patient?> GetByLabIdAsync(string labId, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .FirstOrDefaultAsync(p => p.LabId == labId, cancellationToken);
    }

    public async Task<IReadOnlyList<Patient>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .Where(p => p.DoctorId == doctorId)
            .ToListAsync(cancellationToken);
    }
}
