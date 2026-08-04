using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record CaseUserTrackingDto
{
    public int PatientVisitId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string LabId { get; init; } = string.Empty;
    public DateTime VisitDate { get; init; }
    public string? DoctorName { get; init; }
    public VisitStatus Status { get; init; }
    public int TotalTests { get; init; }
    public int CompletedTests { get; init; }
}
