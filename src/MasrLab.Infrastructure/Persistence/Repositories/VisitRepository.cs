using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class VisitRepository : GenericRepository<PatientVisit>, IVisitRepository
{
    public VisitRepository(MasrLabDbContext context) : base(context)
    {
    }
}
