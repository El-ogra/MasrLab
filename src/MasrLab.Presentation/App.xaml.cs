using MasrLab.Application;
using MasrLab.Infrastructure;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Seeding;
using MasrLab.Infrastructure.Services;
using MasrLab.Presentation.ViewModels;
using MasrLab.Presentation.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Presentation;

public partial class App : System.Windows.Application
{
    private IServiceProvider? _serviceProvider;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();

        _serviceProvider = services.BuildServiceProvider();

        try
        {
            await _serviceProvider.GetRequiredService<IRestoreAccessModeRecovery>().EnsureMultiUserIfNeededAsync();
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(
                $"تعذر التحقق من حالة قاعدة البيانات قبل بدء التطبيق.{Environment.NewLine}{exception.Message}",
                "MasrLab",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            Shutdown();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MasrLabDbContext>();
        await context.Database.MigrateAsync();
        await DefaultAdminSeeder.SeedAsync(context, CancellationToken.None);
        await DefaultSettingsSeeder.SeedAsync(context, CancellationToken.None);
        await DefaultStatisticsSettingsSeeder.SeedAsync(context, CancellationToken.None);
        await DefaultPermissionSeeder.SeedAsync(context, CancellationToken.None);

        bool isFirstRun = await DefaultAdminSeeder.IsFirstRunAsync(context, CancellationToken.None);

        System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

        if (isFirstRun)
        {
            var firstRunWindow = _serviceProvider.GetRequiredService<FirstRunSetupWindow>();
            firstRunWindow.DataContext = _serviceProvider.GetRequiredService<FirstRunSetupViewModel>();
            if (firstRunWindow.ShowDialog() != true)
            {
                Shutdown();
                return;
            }
        }
        else
        {
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.DataContext = _serviceProvider.GetRequiredService<LoginViewModel>();
            if (loginWindow.ShowDialog() != true)
            {
                Shutdown();
                return;
            }
        }

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        System.Windows.Application.Current.MainWindow = mainWindow;
        System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
    }
}
