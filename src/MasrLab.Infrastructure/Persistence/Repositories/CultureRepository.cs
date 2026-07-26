using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class CultureRepository : GenericRepository<Domain.Entities.Culture.Culture>, ICultureRepository
{
    public CultureRepository(MasrLabDbContext context) : base(context)
    {
    }
}
