using MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;
using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatientIntake;

public sealed record RegisterPatientIntakeCommand(
    string Name,
    int AgeYears,
    int AgeMonths,
    int AgeDays,
    AgeUnit AgeUnit,
    Gender Gender,
    string? Phone,
    string? Address,
    string? NationalId,
    string? Notes,
    string LabId,
    int? DoctorId,
    int? ReferralEntityId,
    bool HasDiabetes = false,
    bool OnBloodPressureTreatment = false,
    bool OnAntiviralTreatment = false,
    bool OnAntibiotic = false,
    bool BloodThinning = false,
    bool HasLiverDisease = false,
    bool HasAnemia = false,
    bool HasLupus = false,
    bool HasRenalFailure = false,
    bool HasHypertension = false,
    bool HasJointDisease = false,
    bool RecentContrastOrUltrasound = false,
    bool ConfirmDuplicate = false,
    bool TakenOutsideLab = false,
    bool SpecimenUrine = false,
    bool SpecimenStool = false,
    bool SpecimenBlood = false,
    bool SpecimenSemen = false,
    bool SpecimenCsf = false,
    string Source = "Direct",
    int? TestGroupId = null,
    int? CommercialPackageId = null,
    string? DirectTestIds = null,
    bool ConfirmLargeExpansion = false
) : IRequest<RegisterPatientIntakeResult>;

public sealed record RegisterPatientIntakeResult(
    bool IsRegistered,
    int? PatientId,
    int? PatientVisitId,
    int AttachedTestCount,
    IReadOnlyList<DuplicatePatientDto> PotentialDuplicates)
{
    public bool HasPotentialDuplicates => PotentialDuplicates.Count > 0;
}
