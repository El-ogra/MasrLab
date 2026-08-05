using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Domain.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByPeriodAsync(DateTime start, DateTime end, int? userId, CancellationToken cancellationToken = default);
}
