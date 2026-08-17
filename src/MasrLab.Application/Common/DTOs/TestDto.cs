using MasrLab.Domain.Common.Enums;

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

    public string? TestCode { get; init; }
    public string? HistoryName { get; init; }
    public string? ArabicName { get; init; }
    public string? Branch { get; init; }
    public string? LogGroup { get; init; }
    public string? SampleType { get; init; }
    public bool SeeReport { get; init; }
    public bool PrintWithOther { get; init; }
    public bool AddWithGroup { get; init; }
    public bool IsMainTest { get; init; }
    public int TestTimeDays { get; init; }
    public int ArrangeNo { get; init; }
    public ReferenceType ReferenceType { get; init; }
    public decimal? LabToLabPrice { get; init; }
    public string? BarcodeName { get; init; }
    public string? Tube1 { get; init; }
    public string? Tube2 { get; init; }
    public string? Tube3 { get; init; }
    public bool SentOutsideLab { get; init; }
    public string? OutsourcedLabName { get; init; }
    public decimal? OutsourcedCostPrice { get; init; }
    public decimal? CostPrice { get; init; }
    public string? PatientQuestion { get; init; }

    public bool IsCompound { get; init; }
    public IReadOnlyList<TestComponentDto> Components { get; init; } = Array.Empty<TestComponentDto>();
}
