using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record VisitDto
{
    public int Id { get; init; }
    public DateTime VisitDate { get; init; }
    public int PatientId { get; init; }
    public VisitStatus Status { get; init; }
    public int RegisteredByUserId { get; init; }
    public string LabId { get; init; } = string.Empty;
    public int? DoctorId { get; init; }
    public int? ReferralEntityId { get; init; }
    public bool TakenOutsideLab { get; init; }
}
