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

    public async Task<IReadOnlyList<Patient>> FindProbableDuplicatesAsync(
        string name,
        string? nationalId,
        string? phone,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        var normalizedNationalId = string.IsNullOrWhiteSpace(nationalId) ? null : nationalId.Trim();
        var normalizedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        var hasIdentityMatch = normalizedNationalId is not null || normalizedPhone is not null;

        return await _context.Patients
            .AsNoTracking()
            .Where(p => p.Name == normalizedName &&
                        (!hasIdentityMatch ||
                         (normalizedNationalId != null && p.NationalId == normalizedNationalId) ||
                         (normalizedPhone != null && p.Phone != null && p.Phone.Value == normalizedPhone)))
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

    public async Task<int?> GetMaxLabIdSuffixAsync(string datePrefix, CancellationToken cancellationToken = default)
    {
        var labIds = await _context.Patients
            .FromSqlRaw("SELECT * FROM Patients WITH (UPDLOCK, HOLDLOCK) WHERE LabId LIKE @p0 + '%'", datePrefix)
            .Select(p => p.LabId)
            .ToListAsync(cancellationToken);

        int? max = null;
        foreach (var labId in labIds)
        {
            var dashIndex = labId.LastIndexOf('-');
            if (dashIndex < 0 || dashIndex == labId.Length - 1)
                continue;

            if (int.TryParse(labId.Substring(dashIndex + 1), out var suffix))
            {
                if (max is null || suffix > max.Value)
                    max = suffix;
            }
        }

        return max;
    }
}
