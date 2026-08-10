using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Seeding;

public static class DefaultStatisticsSettingsSeeder
{
    public static async Task SeedAsync(MasrLabDbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.StatisticsSettings.Select(x => x.KpiCode).ToListAsync(cancellationToken);
        foreach (var setting in Defaults.Where(x => !existing.Contains(x.KpiCode)))
            await context.StatisticsSettings.AddAsync(setting, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static readonly StatisticsSetting[] Defaults =
    [
        Create(StatisticsKpiCode.RegisteredPatients, "المرضى المسجلون", 1, KpiValueFormat.Number, 0, KpiComparisonDirection.HigherIsBetter),
        Create(StatisticsKpiCode.RequestedTests, "التحاليل المطلوبة", 2, KpiValueFormat.Number, 0, KpiComparisonDirection.HigherIsBetter),
        Create(StatisticsKpiCode.Revenue, "الإيرادات", 3, KpiValueFormat.Currency, 2, KpiComparisonDirection.HigherIsBetter),
        Create(StatisticsKpiCode.NewPatients, "المرضى الجدد", 4, KpiValueFormat.Number, 0, KpiComparisonDirection.HigherIsBetter),
        Create(StatisticsKpiCode.ReturningPatients, "المرضى العائدون", 5, KpiValueFormat.Number, 0, KpiComparisonDirection.HigherIsBetter),
        Create(StatisticsKpiCode.CollectedSamples, "العينات المسحوبة", 6, KpiValueFormat.Number, 0, KpiComparisonDirection.HigherIsBetter),
        Create(StatisticsKpiCode.PendingSamples, "العينات المعلقة", 7, KpiValueFormat.Number, 0, KpiComparisonDirection.LowerIsBetter),
        Create(StatisticsKpiCode.TopTestDemandRate, "أعلى تحليل طلباً", 8, KpiValueFormat.Percentage, 2, KpiComparisonDirection.Neutral)
    ];

    private static StatisticsSetting Create(StatisticsKpiCode code, string name, short order, KpiValueFormat format, byte decimals, KpiComparisonDirection direction) =>
        new() { KpiCode = code, DisplayName = name, IsEnabled = true, DisplayOrder = order, ValueFormat = format, DecimalPlaces = decimals, ComparisonDirection = direction };
}
