using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class AttendanceLogRepository : GenericRepository<AttendanceLog>, IAttendanceLogRepository
{
    public AttendanceLogRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<AttendanceLog?> GetByUserAndPeriodAsync(int userId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
    {
        return await _context.AttendanceLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(l =>
                l.UserId == userId &&
                l.WorkPeriod.Start >= periodStart &&
                l.WorkPeriod.End <= periodEnd, cancellationToken);
    }
}
