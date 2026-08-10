using MasrLab.Application.Common.Printing;
namespace MasrLab.Infrastructure.Printing.Reports;
public sealed class BlankReport : IReportDefinition
{ public string ReportName => PrintReportNames.BlankResult; public Type PayloadType => typeof(ClinicalReportPrintDto); public byte[] Render(IPrintPayload p) => new ResultDocument((ClinicalReportPrintDto)p, "Blank Report / نموذج فارغ", true).RenderPdf(); }
