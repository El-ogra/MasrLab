using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class Test : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string ReceiptName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public string TurnaroundTime { get; set; } = string.Empty;
    public bool LabToLabFlag { get; set; }
    public string Unit { get; set; } = string.Empty;
}
