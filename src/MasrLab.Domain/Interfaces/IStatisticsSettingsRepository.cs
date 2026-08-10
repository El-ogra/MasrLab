using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Domain.Interfaces;

public interface IStatisticsSettingsRepository
{
    Task<IReadOnlyList<StatisticsSetting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatisticsSetting>> GetByCodesAsync(IReadOnlyCollection<StatisticsKpiCode> codes, CancellationToken cancellationToken = default);
    void Update(StatisticsSetting setting);
}
