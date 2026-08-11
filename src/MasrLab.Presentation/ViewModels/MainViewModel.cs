using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Presentation.ViewModels.PatientManagement;
using MasrLab.Presentation.ViewModels.PatientSearch;
using MasrLab.Presentation.ViewModels.ResultsEntry;
using MasrLab.Presentation.Views.PatientManagement;
using MasrLab.Presentation.Views.PatientSearch;
using MasrLab.Presentation.Views.ResultsEntry;
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

    [RelayCommand]
    private void ShowRegisterPatient()
    {
        IsPatientsSelected = false;
        OnPropertyChanged(nameof(IsPatientsSelected));
        OpenWindow<RegisterPatientView, RegisterPatientViewModel>();
    }

    [RelayCommand]
    private void ShowEnterResults()
    {
        IsPatientsSelected = false;
        OnPropertyChanged(nameof(IsPatientsSelected));
        OpenWindow<EnterResultsView, EnterResultsViewModel>();
    }

    [RelayCommand]
    private void ShowSearchPatients()
    {
        IsPatientsSelected = false;
        OnPropertyChanged(nameof(IsPatientsSelected));
        OpenWindow<SearchPatientsView, SearchPatientsViewModel>();
    }

    [RelayCommand]
    private void ShowDeliverResults()
    {
        IsPatientsSelected = false;
        OnPropertyChanged(nameof(IsPatientsSelected));
        OpenWindow<DeliverResultsView, DeliverResultsViewModel>();
    }

    [RelayCommand]
    private void SelectPatients()
    {
        IsPatientsSelected = true;
        OnPropertyChanged(nameof(IsPatientsSelected));
    }

    protected virtual void OpenWindow<TWindow, TViewModel>()
        where TWindow : System.Windows.Window, new()
        where TViewModel : class
    {
        var window = new TWindow
        {
            DataContext = _serviceProvider.GetRequiredService<TViewModel>()
        };

        var mainWindow = System.Windows.Application.Current?.MainWindow;
        if (mainWindow is not null)
        {
            window.Owner = mainWindow;
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

            IsPatientsSelected = true;
            OnPropertyChanged(nameof(IsPatientsSelected));
        }
    }
}
