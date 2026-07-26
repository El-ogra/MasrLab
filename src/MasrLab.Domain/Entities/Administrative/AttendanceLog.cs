using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Administrative;

public class AttendanceLog : BaseEntity
{
    public int UserId { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public TimeSpan? Overtime { get; set; }
    public TimeSpan? Delays { get; set; }
    public string? BreakPeriods { get; set; }
}
