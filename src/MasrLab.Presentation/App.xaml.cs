using MasrLab.Application;
using MasrLab.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Presentation;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();
        services.AddPresentation();
    }
}
