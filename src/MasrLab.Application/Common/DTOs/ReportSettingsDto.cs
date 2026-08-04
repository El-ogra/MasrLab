using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record ReportSettingsDto
{
    public string Margins { get; init; } = string.Empty;
    public PaperSize PaperSize { get; init; }
    public string? HeaderImage { get; init; }
    public string? HeaderText { get; init; }
    public string? FooterText { get; init; }
    public string? HeaderColor { get; init; }
    public string? FooterColor { get; init; }
}
