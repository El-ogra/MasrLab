namespace MasrLab.Application.Common.Interfaces;

public interface IPrintService
{
    /// <summary>Renders or prints a registered report. The report name must be a value from PrintReportNames
    /// and the payload must match the payload type required by that report.</summary>
    Task PrintAsync(string reportName, object payload, string? printerName = null, CancellationToken ct = default);
    Task<byte[]> RenderAsync(string reportName, object payload, CancellationToken ct = default);
}
