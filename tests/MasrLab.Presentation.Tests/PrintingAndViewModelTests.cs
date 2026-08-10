using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Presentation.Printing;
using MasrLab.Presentation.ViewModels.ResultsEntry;
using Moq;
using MasrLab.Infrastructure.Printing;
using MasrLab.Infrastructure;
using MasrLab.Infrastructure.Printing.Reports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace MasrLab.Presentation.Tests;
public class PrintingAndViewModelTests
{ [Fact] public async Task EnvelopePrinter_DelegatesToPrintService() { var service=new Mock<IPrintService>(); var payload=new EnvelopePrintDto(); await new EnvelopePrinter(service.Object).PrintAsync(payload); service.Verify(x=>x.PrintAsync(PrintReportNames.Envelope,payload,null,It.IsAny<CancellationToken>())); }
 [Fact] public async Task ResultViewModels_PassPayloadDirectly() { var service=new Mock<IPrintService>(); var payload=new ClinicalReportPrintDto(); await new BlankReportViewModel(service.Object).PrintAsync(payload); await new CombinedReportViewModel(service.Object).PrintAsync(payload); service.Verify(x=>x.PrintAsync(PrintReportNames.BlankResult,payload,null,It.IsAny<CancellationToken>())); service.Verify(x=>x.PrintAsync(PrintReportNames.CombinedResult,payload,null,It.IsAny<CancellationToken>())); }
 [Fact] public void Di_ResolvesEveryReportDefinition() { var services=new ServiceCollection(); services.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["ConnectionStrings:MasrLab"]="Server=(localdb)\\MSSQLLocalDB;Database=MasrLabTests;Trusted_Connection=True" }).Build()); using var provider=services.BuildServiceProvider(); using var scope=provider.CreateScope(); Assert.Equal(12,scope.ServiceProvider.GetServices<IReportDefinition>().Count()); Assert.NotNull(scope.ServiceProvider.GetRequiredService<ReportDefinitionRegistry>()); } }
