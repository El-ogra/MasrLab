using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Domain.Interfaces;

public interface IAttendanceLogRepository : IRepository<AttendanceLog>
{
    Task<AttendanceLog?> GetByUserAndPeriodAsync(int userId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default);
}
