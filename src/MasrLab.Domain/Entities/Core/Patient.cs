using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Entities.Core;

public class Patient : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Age Age { get; set; } = new(1, 0, 0);
    public Gender Gender { get; set; }
    public EgyptianPhone? Phone { get; set; }
    public string? Address { get; set; }
    public string? NationalId { get; set; }
    public string? Notes { get; set; }
    public string LabId { get; set; } = string.Empty;
    public int? DoctorId { get; set; }
    public int? ReferralEntityId { get; set; }
    public AccountType AccountType { get; set; }
    public string? DrugAllergy { get; set; }
    public bool Pregnancy { get; set; }
    public bool BloodThinning { get; set; }
    public bool HasDiabetes { get; set; }
    public bool HasHypertension { get; set; }
    public bool HasLiverDisease { get; set; }
    public bool HasJointDisease { get; set; }
    public bool HasRenalFailure { get; set; }
    public bool HasLupus { get; set; }
    public bool HasHeartDisease { get; set; }
    public bool HasThyroidDisorder { get; set; }
    public string? ChronicDiseases { get; set; }
}
