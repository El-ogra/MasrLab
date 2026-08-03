using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Entities.Settings;

public class WorkSheet : BaseEntity
{
    public DateRange Period { get; set; } = new(DateTime.MinValue, DateTime.MaxValue);
    public WorkSheetType Type { get; set; }
    public string? PatientVisitIds { get; set; }
    public string? TestIds { get; set; }
}
