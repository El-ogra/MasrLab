using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Settings;

public class StatisticsSetting : BaseEntity
{
    public StatisticsKpiCode KpiCode { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public short DisplayOrder { get; set; }
    public KpiValueFormat ValueFormat { get; set; }
    public byte DecimalPlaces { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public KpiComparisonDirection ComparisonDirection { get; set; }
}
