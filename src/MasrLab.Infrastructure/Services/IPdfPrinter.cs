namespace MasrLab.Infrastructure.Services;

public interface IPdfPrinter
{
    Task PrintAsync(byte[] pdf, string printerName, CancellationToken ct = default);
}
