using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Presentation.ViewModels.PatientManagement;
using MasrLab.Presentation.ViewModels.PatientSearch;
using MasrLab.Presentation.ViewModels.ResultsEntry;
using MasrLab.Presentation.ViewModels.SystemSettings;
using MasrLab.Presentation.Views.PatientManagement;
using MasrLab.Presentation.Views.PatientSearch;
using MasrLab.Presentation.Views.ResultsEntry;
using MasrLab.Presentation.Views.SystemSettings;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool IsPatientsSelected { get; private set; }
    public bool IsSystemSelected { get; private set; }
    public bool IsTopBarVisible => !IsWindowOpen;
    public bool IsWindowOpen { get; private set; }

    [RelayCommand]
    private void SelectPatients()
    {
        IsPatientsSelected = true;
        IsSystemSelected = false;
        OnPropertyChanged(nameof(IsPatientsSelected));
        OnPropertyChanged(nameof(IsSystemSelected));
    }

    [RelayCommand]
    private void SelectSystem()
    {
        IsSystemSelected = true;
        IsPatientsSelected = false;
        OnPropertyChanged(nameof(IsSystemSelected));
        OnPropertyChanged(nameof(IsPatientsSelected));
    }

    [RelayCommand]
    private void ShowRegisterPatient()
    {
        OpenWindow<RegisterPatientView, RegisterPatientViewModel>();
    }

    [RelayCommand]
    private void ShowEnterResults()
    {
        OpenWindow<EnterResultsView, EnterResultsViewModel>();
    }

    [RelayCommand]
    private void ShowSearchPatients()
    {
        OpenWindow<SearchPatientsView, SearchPatientsViewModel>();
    }

    [RelayCommand]
    private void ShowDeliverResults()
    {
        OpenWindow<DeliverResultsView, DeliverResultsViewModel>();
    }

    [RelayCommand]
    private void ShowTestsMasterData()
    {
        OpenWindow<TestsMasterDataWindow, TestsMasterDataViewModel>();
    }

    [RelayCommand]
    private void ShowBarcodeTypes()
    {
        OpenWindow<BarcodeTypesWindow, BarcodeTypesViewModel>();
    }

    [RelayCommand]
    private void ShowCultureAntibiotics()
    {
        OpenWindow<CultureAntibioticsWindow, CultureAntibioticsViewModel>();
    }

    [RelayCommand]
    private void ShowExternalLabs()
    {
        OpenWindow<ExternalLabsWindow, ExternalLabsViewModel>();
    }

    [RelayCommand]
    private void ShowTestGroups()
    {
        OpenWindow<TestGroupsWindow, TestGroupsViewModel>();
    }

    [RelayCommand]
    private void ShowTestUnits()
    {
        OpenWindow<TestUnitsWindow, TestUnitsViewModel>();
    }

    [RelayCommand]
    private void ShowTestComments()
    {
        OpenWindow<TestCommentsWindow, TestCommentsViewModel>();
    }

    [RelayCommand]
    private void ShowPatientTitles()
    {
        OpenWindow<PatientTitlesWindow, PatientTitlesViewModel>();
    }

    [RelayCommand]
    private void ShowPriceListPrint()
    {
        OpenWindow<PriceListPrintWindow, PriceListPrintViewModel>();
    }

    protected virtual void OpenWindow<TWindow, TViewModel>()
        where TWindow : System.Windows.Window, new()
        where TViewModel : class
    {
        bool previousIsPatientsSelected = IsPatientsSelected;
        bool previousIsSystemSelected = IsSystemSelected;

        IsPatientsSelected = false;
        IsSystemSelected = false;
        OnPropertyChanged(nameof(IsPatientsSelected));
        OnPropertyChanged(nameof(IsSystemSelected));

        var window = new TWindow
        {
            DataContext = _serviceProvider.GetRequiredService<TViewModel>()
        };

        var mainWindow = System.Windows.Application.Current?.MainWindow;
        if (mainWindow is not null)
        {
            window.Owner = mainWindow;
            IsWindowOpen = true;
            OnPropertyChanged(nameof(IsTopBarVisible));
            mainWindow.Hide();
        }

        try
        {
            window.ShowDialog();
        }
        finally
        {
            if (mainWindow is not null)
            {
                mainWindow.Show();
            }

            IsWindowOpen = false;
            OnPropertyChanged(nameof(IsTopBarVisible));

            IsPatientsSelected = previousIsPatientsSelected;
            IsSystemSelected = previousIsSystemSelected;
            OnPropertyChanged(nameof(IsPatientsSelected));
            OnPropertyChanged(nameof(IsSystemSelected));
        }
    }
}
