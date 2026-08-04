namespace MasrLab.Application.Common.DTOs;

public record EnvelopeBarcodeSettingsDto
{
    public string CardTitle { get; init; } = string.Empty;
    public bool ShowPatientName { get; init; } = true;
    public bool ShowLabId { get; init; } = true;
    public bool ShowNationalId { get; init; } = true;
    public bool ShowPhone { get; init; } = true;
    public bool ShowAge { get; init; } = true;
    public bool ShowGender { get; init; } = true;
    public bool ShowAddress { get; init; }
    public string? HeaderColor { get; init; }
    public string? FontSize { get; init; } = "Medium";
}
