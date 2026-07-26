using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Settings;

public class ReportTemplate : BaseEntity
{
    public string Margins { get; set; } = string.Empty;
    public PaperSize PaperSize { get; set; }
    public string? HeaderImage { get; set; }
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string? HeaderColor { get; set; }
    public string? FooterColor { get; set; }
}
