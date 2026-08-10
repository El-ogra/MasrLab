using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Presentation.Printing;
using Moq;

namespace MasrLab.Infrastructure.Tests.Printing;

public class EnvelopePrinterTests
{
    [Fact]
    public async Task PrintAsync_DelegatesEnvelopePayloadToPrintService()
    {
        var service = new Mock<IPrintService>();
        var payload = new EnvelopePrintDto { PatientName = "Ahmed" };
        var printer = new EnvelopePrinter(service.Object);

        await printer.PrintAsync(payload, "Lab Printer");

        service.Verify(x => x.PrintAsync(PrintReportNames.Envelope, payload, "Lab Printer", It.IsAny<CancellationToken>()), Times.Once);
    }
}
