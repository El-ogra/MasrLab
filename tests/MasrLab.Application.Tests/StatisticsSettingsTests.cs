using FluentValidation.TestHelper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.SystemSettings.Commands.UpdateStatisticsSettings;
using MasrLab.Application.Features.SystemSettings.Queries.GetStatisticsSettings;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class StatisticsSettingsTests
{
    [Fact]
    public async Task Query_returns_settings_in_display_order()
    {
        var repository = new Mock<IStatisticsSettingsRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([
            Create(StatisticsKpiCode.Revenue, 2), Create(StatisticsKpiCode.RegisteredPatients, 1)
        ]);
        var result = await new GetStatisticsSettingsQueryHandler(repository.Object).Handle(new(), default);
        Assert.Equal([StatisticsKpiCode.RegisteredPatients, StatisticsKpiCode.Revenue], result.Select(x => x.KpiCode));
    }

    [Fact]
    public async Task Update_updates_fixed_settings_and_saves_once()
    {
        var entity = Create(StatisticsKpiCode.Revenue, 1);
        var repository = new Mock<IStatisticsSettingsRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([entity]);
        var unitOfWork = new Mock<IUnitOfWork>();
        var command = new UpdateStatisticsSettingsCommand([new(StatisticsKpiCode.Revenue, "دخل", true, 1,
            KpiValueFormat.Currency, 2, 10m, 8m, 5m, KpiComparisonDirection.HigherIsBetter)]);
        await new UpdateStatisticsSettingsCommandHandler(repository.Object, unitOfWork.Object).Handle(command, default);
        Assert.Equal("دخل", entity.DisplayName);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_rejects_neutral_kpi_with_thresholds()
    {
        var command = new UpdateStatisticsSettingsCommand([new(StatisticsKpiCode.TopTestDemandRate, "طلب", true, 1,
            KpiValueFormat.Percentage, 2, 10m, null, null, KpiComparisonDirection.Neutral)]);
        var result = new UpdateStatisticsSettingsCommandValidator().TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Settings);
    }

    private static StatisticsSetting Create(StatisticsKpiCode code, short order) => new()
    {
        KpiCode = code, DisplayName = code.ToString(), IsEnabled = true, DisplayOrder = order,
        ValueFormat = KpiValueFormat.Number, ComparisonDirection = KpiComparisonDirection.Neutral
    };
}
