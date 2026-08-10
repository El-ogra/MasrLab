using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using MediatR;

namespace MasrLab.Presentation.ViewModels;

public partial class FirstRunSetupViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    public FirstRunSetupViewModel(IMediator mediator)
    {
        _mediator = mediator;
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
        if (string.IsNullOrWhiteSpace(AdminUsername) || string.IsNullOrWhiteSpace(AdminPassword))
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

            if (System.Windows.Application.Current.MainWindow is System.Windows.Window setupWindow)
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
