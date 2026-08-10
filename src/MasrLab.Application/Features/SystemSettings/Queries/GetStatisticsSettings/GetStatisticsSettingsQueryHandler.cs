using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Queries.GetStatisticsSettings;

public class GetStatisticsSettingsQueryHandler(IStatisticsSettingsRepository repository) : IRequestHandler<GetStatisticsSettingsQuery, IReadOnlyList<StatisticsSettingDto>>
{
    public async Task<IReadOnlyList<StatisticsSettingDto>> Handle(GetStatisticsSettingsQuery request, CancellationToken cancellationToken) =>
        (await repository.GetAllAsync(cancellationToken)).OrderBy(x => x.DisplayOrder).Select(x => new StatisticsSettingDto
        {
            KpiCode = x.KpiCode, DisplayName = x.DisplayName, IsEnabled = x.IsEnabled, DisplayOrder = x.DisplayOrder,
            ValueFormat = x.ValueFormat, DecimalPlaces = x.DecimalPlaces, TargetValue = x.TargetValue,
            WarningThreshold = x.WarningThreshold, CriticalThreshold = x.CriticalThreshold, ComparisonDirection = x.ComparisonDirection
        }).ToList();
}
