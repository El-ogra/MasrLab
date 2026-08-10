using MasrLab.Application.Common.Printing;
using MasrLab.Infrastructure.Printing.Reports;
using UglyToad.PdfPig;

namespace MasrLab.Infrastructure.Tests.Printing;

public class ReceiptReportTests
{
    [Fact]
    public void Render_CreatesPdfContainingReceiptData()
    {
        var pdf = new ReceiptReport().Render(new ReceiptPrintDto
        {
            ReceiptId = 7, ReceiptNumber = "REC-000007", IssueDate = new DateTime(2026, 8, 10),
            PatientName = "Ahmed Ali", PatientLabId = "LAB-7",
            Lines = [new ReceiptPrintLineDto("CBC", 125m)], GrossTotal = 125m, Total = 125m
        });

        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
        using var document = PdfDocument.Open(pdf);
        var text = string.Join(Environment.NewLine, document.GetPages().Select(page => page.Text));
        Assert.Contains("REC-000007", text);
        Assert.Contains("Ahmed Ali", text);
    }
}
