using MasrLab.Application.Common.Printing;
using MasrLab.Infrastructure.Printing;
using MasrLab.Infrastructure.Printing.Reports;

namespace MasrLab.Infrastructure.Tests.Printing;

public class EnvelopeReportTests
{
    [Theory]
    [InlineData(PrintReportNames.Envelope)]
    [InlineData(PrintReportNames.IndividualResult)]
    [InlineData(PrintReportNames.CombinedResult)]
    [InlineData(PrintReportNames.BlankResult)]
    [InlineData(PrintReportNames.CultureResult)]
    public void RegisteredClinicalReports_RenderValidPdf(string name)
    {
        var reports = new IReportDefinition[] { new EnvelopeReport(), new IndividualResultReport(), new CombinedReport(), new BlankReport(), new CultureReport() };
        var report = new ReportDefinitionRegistry(reports).Get(name);
        IPrintPayload payload = name == PrintReportNames.Envelope
            ? new EnvelopePrintDto { PatientName = "Ahmed Ali", PatientCode = "P-1", LaboratoryNumber = "L-1", VisitNumber = 1, DeliveryDate = DateTime.Today, DeliveryTicketNumber = "DLV-1" }
            : new ClinicalReportPrintDto { PatientName = "Ahmed Ali", LaboratoryNumber = "L-1", VisitDate = DateTime.Today, Results = [new ClinicalResultLineDto("CBC", "10", "g/dL", "12-16")] };
        var pdf = report.Render(payload);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
    }
}
