using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence.Repositories;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class RequestAuditLogRepository : GenericRepository<RequestAuditLog>, IRequestAuditLogRepository
{
    public RequestAuditLogRepository(MasrLabDbContext context) : base(context)
    {
    }
}
