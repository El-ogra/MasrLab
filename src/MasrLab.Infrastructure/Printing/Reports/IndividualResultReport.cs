using MasrLab.Application.Common.Printing;
using MasrLab.Infrastructure.Printing.Templates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
namespace MasrLab.Infrastructure.Printing.Reports;
public sealed class IndividualResultReport : IReportDefinition
{ public string ReportName => PrintReportNames.IndividualResult; public Type PayloadType => typeof(ClinicalReportPrintDto); public byte[] Render(IPrintPayload p) => new ResultDocument((ClinicalReportPrintDto)p, "Individual Result / نتيجة فردية", false).RenderPdf(); }

internal sealed class ResultDocument(ClinicalReportPrintDto data, string title, bool blank) : ReportBaseTemplate
{
    protected override string Title => title;
    protected override void ComposeContent(IContainer c) => c.Column(x => { x.Spacing(8); x.Item().Text($"Patient: {data.PatientName}   Lab No: {data.LaboratoryNumber}   Date: {data.VisitDate:yyyy-MM-dd}"); x.Item().Table(t => { t.ColumnsDefinition(d => { d.RelativeColumn(3); d.RelativeColumn(); d.RelativeColumn(); d.RelativeColumn(2); }); t.Header(h => { h.Cell().Text("Test").SemiBold(); h.Cell().Text("Result").SemiBold(); h.Cell().Text("Unit").SemiBold(); h.Cell().Text("Reference").SemiBold(); }); foreach (var r in data.Results) { t.Cell().Text(r.TestName); t.Cell().Text(blank ? "" : r.Value); t.Cell().Text(r.Unit); t.Cell().Text(r.ReferenceRange); } }); if (!blank) { foreach (var r in data.Results.Where(rr => !string.IsNullOrWhiteSpace(rr.Comment))) x.Item().PaddingTop(4).Text($"{r.TestName} Note: {r.Comment}").FontSize(8).Italic(); } if (!blank && !string.IsNullOrWhiteSpace(data.CultureSummary)) x.Item().PaddingTop(12).Text($"Culture: {data.CultureSummary}"); });
}
