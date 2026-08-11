using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.AttendanceAndAudit.Commands.RecordLogin;
using MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using MasrLab.Presentation.Views;
using MediatR;

namespace MasrLab.Presentation.ViewModels;

public partial class FirstRunSetupViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly IAuthenticationService _authenticationService;

    public FirstRunSetupViewModel(IMediator mediator, IAuthenticationService authenticationService)
    {
        _mediator = mediator;
        _authenticationService = authenticationService;
    }

    [ObservableProperty]
    private string _adminUsername = string.Empty;

    [ObservableProperty]
    private string _adminPassword = string.Empty;

    [ObservableProperty]
    private string _adminPasswordConfirm = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(AdminUsername) ||
            string.IsNullOrWhiteSpace(AdminPassword) ||
            string.IsNullOrWhiteSpace(AdminPasswordConfirm))
        {
            ErrorMessage = "يرجى إدخال اسم المستخدم وكلمة المرور";
            return;
        }

        if (AdminPassword != AdminPasswordConfirm)
        {
            ErrorMessage = "كلمتا المرور غير متطابقتين";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            await _mediator.Send(new CreateUserCommand(AdminUsername, AdminPassword, IsAdmin: true));

            var authResult = await _authenticationService.LoginAsync(AdminUsername, AdminPassword);

            if (!authResult.IsSuccess)
            {
                ErrorMessage = authResult.FailureReason;
                return;
            }

            await _mediator.Send(new RecordLoginCommand(authResult.UserId!.Value, DateTime.UtcNow));

            var setupWindow = System.Windows.Application.Current?.Windows
                .OfType<FirstRunSetupWindow>()
                .FirstOrDefault();
            if (setupWindow is not null)
            {
                setupWindow.DialogResult = true;
                setupWindow.Close();
            }
        }
        catch (Exception)
        {
            ErrorMessage = "حدث خطأ أثناء حفظ الإعدادات";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
