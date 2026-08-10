using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Presentation.ViewModels;

public partial class FirstRunSetupViewModel : ObservableObject
{
    private readonly MasrLabDbContext _context;

    public FirstRunSetupViewModel(MasrLabDbContext context)
    {
        _context = context;
    }

    [ObservableProperty]
    private string _adminUsername = string.Empty;

    [ObservableProperty]
    private string _adminPassword = string.Empty;

    [ObservableProperty]
    private string _adminPasswordConfirm = string.Empty;

    [ObservableProperty]
    private string _labName = string.Empty;

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
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(AdminPassword);

            var adminUser = new User
            {
                Username = AdminUsername,
                Password = hashedPassword,
                IsAdmin = true,
                IsActive = true
            };
            _context.Users.Add(adminUser);

            if (!string.IsNullOrWhiteSpace(LabName))
            {
                var existingSetting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == "LabName" && !s.IsDeleted);

                if (existingSetting is null)
                {
                    _context.SystemSettings.Add(new SystemSetting
                    {
                        SettingKey = "LabName",
                        SettingValue = LabName
                    });
                }
            }

            await _context.SaveChangesAsync(CancellationToken.None);

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
