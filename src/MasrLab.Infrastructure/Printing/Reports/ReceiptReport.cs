using MasrLab.Application.Common.Printing;
using MasrLab.Infrastructure.Printing.Templates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace MasrLab.Infrastructure.Printing.Reports;

public sealed class ReceiptReport : IReportDefinition
{
    public string ReportName => PrintReportNames.Receipt;
    public Type PayloadType => typeof(ReceiptPrintDto);

    public byte[] Render(IPrintPayload payload)
    {
        if (payload is not ReceiptPrintDto receipt)
            throw new ArgumentException($"Report '{ReportName}' requires {nameof(ReceiptPrintDto)}.", nameof(payload));
        return new ReceiptDocument(receipt).RenderPdf();
    }

    private sealed class ReceiptDocument(ReceiptPrintDto receipt) : ReportBaseTemplate
    {
        protected override string Title => "Receipt / إيصال";

        protected override void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(8);
                column.Item().Text($"{receipt.ReceiptNumber}   •   {receipt.IssueDate:yyyy-MM-dd HH:mm}");
                column.Item().Text($"Patient / المريض: {receipt.PatientName}");
                column.Item().Text($"Lab ID: {receipt.PatientLabId}" +
                    (string.IsNullOrWhiteSpace(receipt.PatientPhone) ? string.Empty : $"   •   {receipt.PatientPhone}"));
                column.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns => { columns.RelativeColumn(4); columns.RelativeColumn(1); });
                    table.Header(header =>
                    {
                        header.Cell().Text("Service").SemiBold();
                        header.Cell().AlignRight().Text("Amount").SemiBold();
                    });
                    foreach (var line in receipt.Lines)
                    {
                        table.Cell().Text(line.Description);
                        table.Cell().AlignRight().Text(FormatMoney(line.Amount));
                    }
                });
                column.Item().PaddingTop(10).AlignRight().Column(totals =>
                {
                    totals.Item().Text($"Gross total: {FormatMoney(receipt.GrossTotal)}");
                    totals.Item().Text($"Discount: {FormatMoney(receipt.Discount)}");
                    totals.Item().Text($"Total: {FormatMoney(receipt.Total)}").SemiBold();
                    totals.Item().Text($"Paid now: {FormatMoney(receipt.PaidNow)}");
                    totals.Item().Text($"Remaining: {FormatMoney(receipt.Remaining)}");
                });
            });
        }

        private string FormatMoney(decimal amount) => $"{amount:N2} {receipt.Currency}";
    }
}
