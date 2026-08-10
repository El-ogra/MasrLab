using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateStatisticsSettings;

public class UpdateStatisticsSettingsCommandHandler(IStatisticsSettingsRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateStatisticsSettingsCommand, Unit>
{
    public async Task<Unit> Handle(UpdateStatisticsSettingsCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAllAsync(cancellationToken);
        if (existing.Count != request.Settings.Count || request.Settings.Any(x => existing.All(s => s.KpiCode != x.KpiCode)))
            throw new InvalidOperationException("Statistics KPI codes are fixed and must match the configured set.");
        foreach (var item in request.Settings)
        {
            var setting = existing.Single(x => x.KpiCode == item.KpiCode);
            setting.DisplayName = item.DisplayName; setting.IsEnabled = item.IsEnabled; setting.DisplayOrder = item.DisplayOrder;
            setting.ValueFormat = item.ValueFormat; setting.DecimalPlaces = item.DecimalPlaces; setting.TargetValue = item.TargetValue;
            setting.WarningThreshold = item.WarningThreshold; setting.CriticalThreshold = item.CriticalThreshold; setting.ComparisonDirection = item.ComparisonDirection;
            repository.Update(setting);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
