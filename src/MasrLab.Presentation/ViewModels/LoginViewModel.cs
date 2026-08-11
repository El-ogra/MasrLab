using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;
using MasrLab.Application.Features.UsersAndPermissions.Queries.GetRegisteredUsernames;
using MasrLab.Presentation.Views;
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
        _ = LoadUsernamesAsync();
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<string> _usernames = new();

    private async Task LoadUsernamesAsync()
    {
        try
        {
            var usernames = await _mediator.Send(new GetRegisteredUsernamesQuery());
            Usernames = new ObservableCollection<string>(usernames);
        }
        catch (Exception)
        {
            ErrorMessage = "فشل تحميل قائمة المستخدمين";
        }
    }

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

            if (!result.IsSuccess)
            {
                ErrorMessage = result.FailureReason;
                return;
            }

            await _mediator.Send(new RecordLoginCommand(result.UserId!.Value, DateTime.UtcNow));

            var loginWindow = System.Windows.Application.Current?.Windows
                .OfType<LoginWindow>()
                .FirstOrDefault();
            if (loginWindow is not null)
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
