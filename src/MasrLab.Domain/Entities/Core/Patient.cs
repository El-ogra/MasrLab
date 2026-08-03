using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;
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

    public static Patient Register(string name, string labId, int? doctorId = null, int? referralEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("Patient name cannot be empty.");
        var patient = new Patient
        {
            Name = name,
            LabId = labId,
            DoctorId = doctorId,
            ReferralEntityId = referralEntityId
        };
        patient.AddDomainEvent(new PatientRegistered(patient.Id, name));
        return patient;
    }

    public void UpdateProfile(string? name, string? address, string? notes, string? nationalId)
    {
        var changedFields = new List<string>();
        if (name is not null && name != Name)
        {
            Name = name;
            changedFields.Add(nameof(Name));
        }
        if (address is not null && address != Address)
        {
            Address = address;
            changedFields.Add(nameof(Address));
        }
        if (notes is not null && notes != Notes)
        {
            Notes = notes;
            changedFields.Add(nameof(Notes));
        }
        if (nationalId is not null && nationalId != NationalId)
        {
            NationalId = nationalId;
            changedFields.Add(nameof(NationalId));
        }
        if (changedFields.Count == 0)
            return;
        AddDomainEvent(new PatientUpdated(Id, changedFields.ToArray()));
    }
}
