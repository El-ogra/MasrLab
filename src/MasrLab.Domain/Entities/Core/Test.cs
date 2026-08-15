using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

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

    public string? TestCode { get; set; }
    public string? HistoryName { get; set; }
    public string? ArabicName { get; set; }
    public string? Branch { get; set; }
    public string? LogGroup { get; set; }
    public string? SampleType { get; set; }
    public bool SeeReport { get; set; }
    public bool PrintWithOther { get; set; }
    public bool AddWithGroup { get; set; }
    public bool IsMainTest { get; set; }
    public int TestTimeDays { get; set; }
    public int ArrangeNo { get; set; }
    public ReferenceType ReferenceType { get; set; }
    public decimal? LabToLabPrice { get; set; }
    public string? BarcodeName { get; set; }
    public string? Tube1 { get; set; }
    public string? Tube2 { get; set; }
    public string? Tube3 { get; set; }
    public bool SentOutsideLab { get; set; }
    public string? OutsourcedLabName { get; set; }
    public decimal? OutsourcedCostPrice { get; set; }
    public string? PatientQuestion { get; set; }

    public ICollection<ReferenceValue> ReferenceValues { get; set; } = new List<ReferenceValue>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TestComponent> TestComponents { get; set; } = new List<TestComponent>();
}
