using MasrLab.Domain.Common;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Entities.Administrative;

public class AttendanceLog : BaseEntity
{
    public int UserId { get; set; }
    public DateRange WorkPeriod { get; set; } = new(DateTime.MinValue, null);
    public TimeSpan? Overtime { get; set; }
    public TimeSpan? Delays { get; set; }
    public string? BreakPeriods { get; set; }
}
