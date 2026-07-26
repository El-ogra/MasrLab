using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Core;

public class Patient : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int AgeYears { get; set; }
    public int AgeMonths { get; set; }
    public int AgeDays { get; set; }
    public Gender Gender { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? NationalId { get; set; }
    public string? Notes { get; set; }
    public string LabId { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public int ReferralEntityId { get; set; }
    public AccountType AccountType { get; set; }
    public string? DrugAllergy { get; set; }
    public bool Pregnancy { get; set; }
    public bool BloodThinning { get; set; }
    public string? ChronicDiseases { get; set; }
}
