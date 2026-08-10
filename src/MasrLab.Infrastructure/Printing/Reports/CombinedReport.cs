using MasrLab.Application.Common.Printing;
namespace MasrLab.Infrastructure.Printing.Reports;
public sealed class CombinedReport : IReportDefinition
{ public string ReportName => PrintReportNames.CombinedResult; public Type PayloadType => typeof(ClinicalReportPrintDto); public byte[] Render(IPrintPayload p) => new ResultDocument((ClinicalReportPrintDto)p, "Combined Results / نتائج مجمعة", false).RenderPdf(); }
