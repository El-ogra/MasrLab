using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Settings;

public class WorkSheet : BaseEntity
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public WorkSheetType Type { get; set; }
    public string? PatientVisitIds { get; set; }
    public string? TestIds { get; set; }
}
