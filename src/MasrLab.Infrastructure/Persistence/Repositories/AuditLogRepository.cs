using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.ActionTime >= start && a.ActionTime <= end)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByPeriodAsync(DateTime start, DateTime end, int? userId, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.ActionTime >= start && a.ActionTime <= end);

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
