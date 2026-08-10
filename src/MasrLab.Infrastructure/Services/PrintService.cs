using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MasrLab.Infrastructure.Services;

public class PrintService : IPrintService
{
    private const string DefaultPrinterSettingKey = "DefaultPrinter";
    private readonly ISystemSettingRepository _settingRepository;
    private readonly IPdfPrinter _pdfPrinter;

    public PrintService(ISystemSettingRepository settingRepository, IPdfPrinter pdfPrinter)
    {
        _settingRepository = settingRepository;
        _pdfPrinter = pdfPrinter;
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

        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(text => text.FontSize(12));
                    page.Header().Text(reportName).SemiBold().FontSize(18);
                    page.Content().PaddingVertical(20).Text(payload?.ToString() ?? string.Empty);
                    page.Footer().AlignCenter().Text("MasrLab reference report");
                });
            }).GeneratePdf();
        }, ct);
    }
}
