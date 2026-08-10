using FluentValidation;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateStatisticsSettings;

public class UpdateStatisticsSettingsCommandValidator : AbstractValidator<UpdateStatisticsSettingsCommand>
{
    public UpdateStatisticsSettingsCommandValidator()
    {
        RuleFor(x => x.Settings).NotEmpty();
        RuleForEach(x => x.Settings).SetValidator(new UpdateStatisticsSettingItemDtoValidator());
        RuleFor(x => x.Settings.Select(s => s.KpiCode).Distinct().Count()).Equal(x => x.Settings.Count).WithMessage("KPI codes must be unique.");
        RuleFor(x => x.Settings.Select(s => s.DisplayOrder).Distinct().Count()).Equal(x => x.Settings.Count).WithMessage("Display orders must be unique.");
    }

}

public class UpdateStatisticsSettingItemDtoValidator : AbstractValidator<UpdateStatisticsSettingItemDto>
{
    public UpdateStatisticsSettingItemDtoValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DisplayOrder).GreaterThan((short)0);
        RuleFor(x => x.DecimalPlaces).LessThanOrEqualTo((byte)4);
        RuleFor(x => x.TargetValue).GreaterThanOrEqualTo(0).When(x => x.TargetValue.HasValue);
        RuleFor(x => x.WarningThreshold).GreaterThanOrEqualTo(0).When(x => x.WarningThreshold.HasValue);
        RuleFor(x => x.CriticalThreshold).GreaterThanOrEqualTo(0).When(x => x.CriticalThreshold.HasValue);
        RuleFor(x => x).Must(ThresholdsAreValid).WithMessage("KPI thresholds are inconsistent with the comparison direction.");
    }

    private static bool ThresholdsAreValid(UpdateStatisticsSettingItemDto item)
    {
        if (item.ComparisonDirection == KpiComparisonDirection.Neutral)
            return item.TargetValue is null && item.WarningThreshold is null && item.CriticalThreshold is null;
        if (!item.TargetValue.HasValue || !item.WarningThreshold.HasValue || !item.CriticalThreshold.HasValue)
            return true;
        return item.ComparisonDirection == KpiComparisonDirection.HigherIsBetter
            ? item.CriticalThreshold <= item.WarningThreshold && item.WarningThreshold <= item.TargetValue
            : item.TargetValue <= item.WarningThreshold && item.WarningThreshold <= item.CriticalThreshold;
    }
}
