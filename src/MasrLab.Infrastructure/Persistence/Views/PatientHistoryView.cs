namespace MasrLab.Infrastructure.Persistence.Views;

public class PatientHistoryView
{
    public int PatientId { get; set; }
    public string LabId { get; set; } = string.Empty;
    public int TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string TestReportName { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string? PreviousUnit { get; set; }
    public string? PreviousReferenceRange { get; set; }
    public string? PreviousStatus { get; set; }
    public DateTime? PreviousVisitDate { get; set; }
    public string? CurrentValue { get; set; }
    public string? CurrentUnit { get; set; }
    public string? CurrentReferenceRange { get; set; }
    public string? CurrentStatus { get; set; }
    public DateTime CurrentVisitDate { get; set; }
    public bool ComparisonFlag { get; set; }
}
