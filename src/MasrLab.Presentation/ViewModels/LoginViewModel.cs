using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;
using MediatR;

namespace MasrLab.Presentation.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMediator _mediator;

    public LoginViewModel(IAuthenticationService authenticationService, IMediator mediator)
    {
        _authenticationService = authenticationService;
        _mediator = mediator;
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

            await _mediator.Send(new RecordLoginCommand(result.UserId, DateTime.UtcNow));

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
