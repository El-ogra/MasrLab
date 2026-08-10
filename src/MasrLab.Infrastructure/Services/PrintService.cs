using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Printing;

namespace MasrLab.Infrastructure.Services;

public class PrintService : IPrintService
{
    private const string DefaultPrinterSettingKey = "DefaultPrinter";
    private readonly ISystemSettingRepository _settingRepository;
    private readonly IPdfPrinter _pdfPrinter;
    private readonly ReportDefinitionRegistry _reportDefinitions;

    public PrintService(ISystemSettingRepository settingRepository, IPdfPrinter pdfPrinter, ReportDefinitionRegistry reportDefinitions)
    {
        _settingRepository = settingRepository;
        _pdfPrinter = pdfPrinter;
        _reportDefinitions = reportDefinitions;
    }

    public async Task PrintAsync(string reportName, object payload, string? printerName = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportName);
        ct.ThrowIfCancellationRequested();

        var selectedPrinter = printerName;
        if (string.IsNullOrWhiteSpace(selectedPrinter))
        {
            var settings = await _settingRepository.GetByKeysAsync([DefaultPrinterSettingKey], ct);
            selectedPrinter = settings.FirstOrDefault(setting => setting.SettingKey == DefaultPrinterSettingKey)?.SettingValue;
        }

        if (string.IsNullOrWhiteSpace(selectedPrinter))
            throw new InvalidOperationException("No printer was specified and the DefaultPrinter setting is not configured.");

        var pdf = await RenderAsync(reportName, payload, ct);
        await _pdfPrinter.PrintAsync(pdf, selectedPrinter, ct);
    }

    public Task<byte[]> RenderAsync(string reportName, object payload, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportName);
        ct.ThrowIfCancellationRequested();

        if (payload is not IPrintPayload printPayload)
            throw new ArgumentException("A print payload must implement IPrintPayload.", nameof(payload));

        var definition = _reportDefinitions.Get(reportName);
        if (!definition.PayloadType.IsInstanceOfType(printPayload))
            throw new ArgumentException($"Report '{reportName}' requires a payload of type {definition.PayloadType.Name}.", nameof(payload));

        return Task.Run(() => definition.Render(printPayload), ct);
    }
}
