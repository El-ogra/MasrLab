using System.Windows;
using MasrLab.Presentation.ViewModels;

namespace MasrLab.Presentation.Views;

public partial class FirstRunSetupWindow : Window
{
    public FirstRunSetupWindow()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is FirstRunSetupViewModel viewModel)
        {
            viewModel.AdminPassword = PasswordBox.Password;
        }
    }

    private void PasswordConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is FirstRunSetupViewModel viewModel)
        {
            viewModel.AdminPasswordConfirm = PasswordConfirmBox.Password;
        }
    }
}
