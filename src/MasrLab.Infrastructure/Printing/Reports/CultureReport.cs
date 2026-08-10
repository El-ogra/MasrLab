using MasrLab.Application.Common.Printing;
namespace MasrLab.Infrastructure.Printing.Reports;
public sealed class CultureReport : IReportDefinition
{ public string ReportName => PrintReportNames.CultureResult; public Type PayloadType => typeof(ClinicalReportPrintDto); public byte[] Render(IPrintPayload p) => new ResultDocument((ClinicalReportPrintDto)p, "Culture Report / تقرير مزرعة", false).RenderPdf(); }
