using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.SystemSettings.Commands.UpdateStatisticsSettings;
using MasrLab.Application.Features.SystemSettings.Queries.GetStatisticsSettings;
using MediatR;
using System.Collections.ObjectModel;

namespace MasrLab.Presentation.ViewModels.Statistics;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    public ObservableCollection<StatisticsSettingDto> Settings { get; } = [];

    public StatisticsViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }

    private async Task LoadAsync()
    {
        var settings = await _mediator.Send(new GetStatisticsSettingsQuery());
        Settings.Clear();
        foreach (var setting in settings)
            Settings.Add(setting);
    }

    private Task SaveAsync() => _mediator.Send(new UpdateStatisticsSettingsCommand(
        Settings.Select(x => new UpdateStatisticsSettingItemDto(x.KpiCode, x.DisplayName, x.IsEnabled, x.DisplayOrder,
            x.ValueFormat, x.DecimalPlaces, x.TargetValue, x.WarningThreshold, x.CriticalThreshold, x.ComparisonDirection)).ToList()));
}
