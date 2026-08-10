using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;

namespace MasrLab.Presentation.Printing;

public sealed class EnvelopePrinter(IPrintService printService)
{
    public Task PrintAsync(EnvelopePrintDto payload, string? printerName = null, CancellationToken ct = default) =>
        printService.PrintAsync(PrintReportNames.Envelope, payload, printerName, ct);
}
