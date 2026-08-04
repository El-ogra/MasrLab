namespace MasrLab.Application.Common.DTOs;

public record ReceiptSettingsDto
{
    public string? HeaderImage { get; init; }
    public string? HeaderText { get; init; }
    public string? FooterText { get; init; }
    public string? HeaderColor { get; init; }
    public string? FooterColor { get; init; }
}
