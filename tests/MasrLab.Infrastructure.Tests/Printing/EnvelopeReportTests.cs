using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Infrastructure.Printing;
using MasrLab.Infrastructure.Printing.Reports;
using MasrLab.Infrastructure.Services;
using Moq;

namespace MasrLab.Infrastructure.Tests.Printing;

public class EnvelopeReportTests
{
    [Fact]
    public void EnvelopeReport_GeneratesPatientCodeBarcode_AndRendersValidPdf()
    {
        var barcodeService = new Mock<IBarcodeService>();
        barcodeService.Setup(service => service.GenerateBarcode("P-1", 300, 100))
            .Returns(new BarcodeService().GenerateBarcode("P-1", 300, 100));
        var report = new EnvelopeReport(barcodeService.Object);

        var pdf = report.Render(new EnvelopePrintDto
        {
            PatientName = "Ahmed Ali",
            PatientCode = "P-1",
            LaboratoryNumber = "L-1",
            BarcodeSettings = new EnvelopeBarcodeSettingsDto { UseBarcode = true, BarcodeWidth = 300, BarcodeHeight = 100 }
        });

        barcodeService.Verify(service => service.GenerateBarcode("P-1", 300, 100), Times.Once);
        Assert.True(pdf.Length > 5);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
    }

    [Fact]
    public void EnvelopeReport_DoesNotGenerateBarcode_WhenDisabled()
    {
        var barcodeService = new Mock<IBarcodeService>(MockBehavior.Strict);
        var report = new EnvelopeReport(barcodeService.Object);

        var pdf = report.Render(new EnvelopePrintDto { PatientName = "Ahmed Ali", PatientCode = "P-1" });

        barcodeService.VerifyNoOtherCalls();
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
    }

    [Fact]
    public void EnvelopeReport_RejectsMissingPatientCode_WhenBarcodeIsEnabled()
    {
        var report = new EnvelopeReport(new Mock<IBarcodeService>().Object);

        Assert.Throws<ArgumentException>(() => report.Render(new EnvelopePrintDto
        {
            BarcodeSettings = new EnvelopeBarcodeSettingsDto { UseBarcode = true }
        }));
    }

    [Theory]
    [InlineData(PrintReportNames.Envelope)]
    [InlineData(PrintReportNames.IndividualResult)]
    [InlineData(PrintReportNames.CombinedResult)]
    [InlineData(PrintReportNames.BlankResult)]
    [InlineData(PrintReportNames.CultureResult)]
    public void RegisteredClinicalReports_RenderValidPdf(string name)
    {
        var reports = new IReportDefinition[] { new EnvelopeReport(new BarcodeService()), new IndividualResultReport(), new CombinedReport(), new BlankReport(), new CultureReport() };
        var report = new ReportDefinitionRegistry(reports).Get(name);
        IPrintPayload payload = name == PrintReportNames.Envelope
            ? new EnvelopePrintDto { PatientName = "Ahmed Ali", PatientCode = "P-1", LaboratoryNumber = "L-1", VisitNumber = 1, DeliveryDate = DateTime.Today, DeliveryTicketNumber = "DLV-1" }
            : new ClinicalReportPrintDto { PatientName = "Ahmed Ali", LaboratoryNumber = "L-1", VisitDate = DateTime.Today, Results = [new ClinicalResultLineDto("CBC", "10", "g/dL", "12-16")] };
        var pdf = report.Render(payload);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
    }
}
