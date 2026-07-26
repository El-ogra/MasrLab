using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(MasrLabDbContext context) : base(context)
    {
    }
}
