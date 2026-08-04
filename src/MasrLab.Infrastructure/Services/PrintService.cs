using MasrLab.Application.Common.Interfaces;

namespace MasrLab.Infrastructure.Services;

public class PrintService : IPrintService
{
    public Task PrintAsync(string reportName, object payload, string? printerName = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> RenderAsync(string reportName, object payload, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
