using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(MasrLabDbContext context) : base(context)
    {
    }
}
