using MasrLab.Application;
using MasrLab.Infrastructure;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Seeding;
using MasrLab.Presentation.ViewModels;
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

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MasrLabDbContext>();
        await context.Database.EnsureCreatedAsync();
        await DefaultAdminSeeder.SeedAsync(context);
        await DefaultSettingsSeeder.SeedAsync(context);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }
}
