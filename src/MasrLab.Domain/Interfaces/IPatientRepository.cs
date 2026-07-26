using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<IReadOnlyList<Patient>> SearchByNameAsync(string name);
    Task<Patient?> GetByLabIdAsync(string labId);
    Task<IReadOnlyList<Patient>> GetByDoctorIdAsync(int doctorId);
}
