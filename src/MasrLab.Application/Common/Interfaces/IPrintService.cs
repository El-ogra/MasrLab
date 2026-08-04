namespace MasrLab.Application.Common.Interfaces;

public interface IPrintService
{
    Task PrintAsync(string reportName, object payload, string? printerName = null, CancellationToken ct = default);
    Task<byte[]> RenderAsync(string reportName, object payload, CancellationToken ct = default);
}
