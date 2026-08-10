using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Infrastructure.Printing;
using MasrLab.Infrastructure.Printing.Reports;
using MasrLab.Infrastructure.Services;
using Moq;

namespace MasrLab.Infrastructure.Tests;

public class PrintServiceTests
{
    [Fact]
    public async Task RenderAsync_ReturnsAPdfDocument()
    {
        var service = CreateService(out _, out _);

        var pdf = await service.RenderAsync(PrintReportNames.Receipt, ReceiptPayload());

        Assert.True(pdf.Length >= 5);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
    }

    [Fact]
    public async Task PrintAsync_UsesConfiguredDefaultPrinterWhenNoPrinterIsSupplied()
    {
        var service = CreateService(out var settings, out var printer, "Laboratory Printer");

        await service.PrintAsync(PrintReportNames.Receipt, ReceiptPayload());

        settings.Verify(repository => repository.GetByKeysAsync(
            It.Is<IReadOnlyCollection<string>>(keys => keys.Contains("DefaultPrinter")),
            It.IsAny<CancellationToken>()), Times.Once);
        printer.Verify(dispatcher => dispatcher.PrintAsync(
            It.Is<byte[]>(pdf => pdf.Length > 5 && System.Text.Encoding.ASCII.GetString(pdf, 0, 5) == "%PDF-"),
            "Laboratory Printer",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PrintAsync_ThrowsWhenNoPrinterWasSpecifiedOrConfigured()
    {
        var service = CreateService(out _, out var printer);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.PrintAsync(PrintReportNames.Receipt, ReceiptPayload()));

        printer.Verify(dispatcher => dispatcher.PrintAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static PrintService CreateService(
        out Mock<ISystemSettingRepository> settings,
        out Mock<IPdfPrinter> printer,
        string? defaultPrinter = null)
    {
        settings = new Mock<ISystemSettingRepository>();
        settings.Setup(repository => repository.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultPrinter is null
                ? []
                : [new SystemSetting { SettingKey = "DefaultPrinter", SettingValue = defaultPrinter }]);

        printer = new Mock<IPdfPrinter>();
        printer.Setup(dispatcher => dispatcher.PrintAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new PrintService(settings.Object, printer.Object, new ReportDefinitionRegistry([new ReceiptReport()]));
    }

    private static ReceiptPrintDto ReceiptPayload() => new()
    {
        ReceiptId = 1, ReceiptNumber = "REC-000001", IssueDate = DateTime.UtcNow,
        PatientName = "Test Patient", PatientLabId = "LAB-01",
        Lines = [new ReceiptPrintLineDto("CBC", 100m)], GrossTotal = 100m,
        Total = 100m, Currency = "EGP"
    };
}
