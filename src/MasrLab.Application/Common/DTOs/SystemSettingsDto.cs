namespace MasrLab.Application.Common.DTOs;

public record SystemSettingsDto
{
    public ReceiptSettingsDto ReceiptSettings { get; init; } = new();
    public ReportSettingsDto ReportSettings { get; init; } = new();
    public AccountSettingsDto AccountSettings { get; init; } = new();
    public PrinterDto Printer { get; init; } = new();
    public EnvelopeBarcodeSettingsDto EnvelopeBarcodeSettings { get; init; } = new();
}
