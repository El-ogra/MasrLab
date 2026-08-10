using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.Interfaces;

namespace MasrLab.Presentation.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "يرجى إدخال اسم المستخدم وكلمة المرور";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _authenticationService.LoginAsync(Username, Password);

            if (result.UserId == 0)
            {
                ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";
                return;
            }

            if (System.Windows.Application.Current.MainWindow is System.Windows.Window loginWindow)
            {
                loginWindow.DialogResult = true;
                loginWindow.Close();
            }
        }
        catch (Exception)
        {
            ErrorMessage = "حدث خطأ أثناء تسجيل الدخول";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
