namespace MasrLab.Application.Common.DTOs;

public record EnvelopeBarcodeSettingsDto
{
    public bool UseBarcode { get; init; }
    public int BarcodeWidth { get; init; } = 300;
    public int BarcodeHeight { get; init; } = 100;
}
