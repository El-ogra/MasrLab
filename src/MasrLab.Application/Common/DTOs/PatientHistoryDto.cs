namespace MasrLab.Application.Common.DTOs;

public record PatientHistoryDto
{
    public int PatientId { get; init; }
    public string LabId { get; init; } = string.Empty;
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public string TestReportName { get; init; } = string.Empty;
    public string PreviousValue { get; init; } = string.Empty;
    public string PreviousUnit { get; init; } = string.Empty;
    public string PreviousReferenceRange { get; init; } = string.Empty;
    public string PreviousStatus { get; init; } = string.Empty;
    public DateTime? PreviousVisitDate { get; init; }
    public string CurrentValue { get; init; } = string.Empty;
    public string CurrentUnit { get; init; } = string.Empty;
    public string CurrentReferenceRange { get; init; } = string.Empty;
    public string CurrentStatus { get; init; } = string.Empty;
    public DateTime CurrentVisitDate { get; init; }
    public bool ComparisonFlag { get; init; }
}
