using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Patient>> SearchByNameAsync(string name)
    {
        return await _context.Patients
            .Where(p => p.Name.Contains(name))
            .ToListAsync();
    }

    public async Task<Patient?> GetByLabIdAsync(string labId)
    {
        return await _context.Patients
            .FirstOrDefaultAsync(p => p.LabId == labId);
    }

    public async Task<IReadOnlyList<Patient>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Patients
            .Where(p => p.DoctorId == doctorId)
            .ToListAsync();
    }
}
