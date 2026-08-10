using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class StatisticsSettingsRepository(MasrLabDbContext context) : IStatisticsSettingsRepository
{
    public async Task<IReadOnlyList<StatisticsSetting>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.StatisticsSettings.OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StatisticsSetting>> GetByCodesAsync(IReadOnlyCollection<StatisticsKpiCode> codes, CancellationToken cancellationToken = default) =>
        await context.StatisticsSettings.Where(x => codes.Contains(x.KpiCode)).ToListAsync(cancellationToken);

    public void Update(StatisticsSetting setting) => context.StatisticsSettings.Update(setting);
}
