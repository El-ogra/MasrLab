using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record UpdateStatisticsSettingItemDto(StatisticsKpiCode KpiCode, string DisplayName, bool IsEnabled, short DisplayOrder, KpiValueFormat ValueFormat, byte DecimalPlaces, decimal? TargetValue, decimal? WarningThreshold, decimal? CriticalThreshold, KpiComparisonDirection ComparisonDirection);
