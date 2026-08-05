using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<IReadOnlyList<Patient>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Patient?> GetByLabIdAsync(string labId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Patient>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default);
}
