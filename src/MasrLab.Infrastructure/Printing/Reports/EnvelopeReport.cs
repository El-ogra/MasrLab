using MasrLab.Application.Common.Printing;
using MasrLab.Infrastructure.Printing.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MasrLab.Infrastructure.Printing.Reports;

public sealed class EnvelopeReport : IReportDefinition
{
    public string ReportName => PrintReportNames.Envelope;
    public Type PayloadType => typeof(EnvelopePrintDto);
    public byte[] Render(IPrintPayload payload) => new Document((EnvelopePrintDto)payload).RenderPdf();
    private sealed class Document(EnvelopePrintDto data) : ReportBaseTemplate
    {
        protected override string Title => "MasrLab";
        protected override PageSize PageSize => new(680, 320);
        protected override void ComposeContent(IContainer c) => c.Border(1).Padding(25).AlignCenter().Column(x =>
        { x.Spacing(12); x.Item().Text(data.PatientName).FontSize(22).SemiBold(); x.Item().Text($"كود المريض: {data.PatientCode}"); x.Item().Text($"رقم المعمل: {data.LaboratoryNumber}"); x.Item().Text($"رقم الزيارة: {data.VisitNumber}"); x.Item().Text($"تاريخ التسليم: {data.DeliveryDate:yyyy-MM-dd}"); x.Item().Text($"تذكرة التسليم: {data.DeliveryTicketNumber}"); });
    }
}
