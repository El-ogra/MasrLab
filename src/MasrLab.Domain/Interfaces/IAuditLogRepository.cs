using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Domain.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(int userId);
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime start, DateTime end);
}
