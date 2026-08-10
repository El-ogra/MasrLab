using System.Diagnostics;

namespace MasrLab.Infrastructure.Services;

public sealed class WindowsPdfPrinter : IPdfPrinter
{
    public async Task PrintAsync(byte[] pdf, string printerName, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pdf);
        ArgumentException.ThrowIfNullOrWhiteSpace(printerName);
        ct.ThrowIfCancellationRequested();

        var filePath = Path.Combine(Path.GetTempPath(), $"MasrLab-{Guid.NewGuid():N}.pdf");
        await File.WriteAllBytesAsync(filePath, pdf, ct);

        try
        {
            using var process = Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true,
                Verb = "printto",
                Arguments = $"\"{printerName}\""
            }) ?? throw new InvalidOperationException("Unable to start the operating system print handler.");

            await process.WaitForExitAsync(ct);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
