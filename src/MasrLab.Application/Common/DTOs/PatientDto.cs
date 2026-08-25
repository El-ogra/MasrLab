using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record PatientDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int AgeYears { get; init; }
    public int AgeMonths { get; init; }
    public int AgeDays { get; init; }
    public Gender Gender { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? NationalId { get; init; }
    public string? Notes { get; init; }
    public string LabId { get; init; } = string.Empty;
    public int? DoctorId { get; init; }
    public int? ReferralEntityId { get; init; }
    public bool HasDiabetes { get; init; }
    public bool OnBloodPressureTreatment { get; init; }
    public bool OnAntiviralTreatment { get; init; }
    public bool OnAntibiotic { get; init; }
    public bool BloodThinning { get; init; }
    public bool HasLiverDisease { get; init; }
    public bool HasAnemia { get; init; }
    public bool HasLupus { get; init; }
    public bool HasRenalFailure { get; init; }
    public bool HasHypertension { get; init; }
    public bool HasJointDisease { get; init; }
    public bool RecentContrastOrUltrasound { get; init; }
}
