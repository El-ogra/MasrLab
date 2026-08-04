namespace MasrLab.Application.Common.DTOs;

public record TestDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ReportName { get; init; } = string.Empty;
    public string ReceiptName { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public decimal Price { get; init; }
    public string TurnaroundTime { get; init; } = string.Empty;
    public bool LabToLabFlag { get; init; }
    public string Unit { get; init; } = string.Empty;
}
