using MasrLab.Presentation.ViewModels.SystemSettings;

namespace MasrLab.Presentation.Views.SystemSettings;

public partial class TestsMasterDataWindow : System.Windows.Window
{
    public TestsMasterDataWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is TestsMasterDataViewModel viewModel)
        {
            await viewModel.LoadTestsCommand.ExecuteAsync(null);
        }
    }
}
