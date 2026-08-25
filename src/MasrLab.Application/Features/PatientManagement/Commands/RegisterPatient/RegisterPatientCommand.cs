using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public record RegisterPatientCommand(
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
    bool RecentContrastOrUltrasound = false
) : IRequest<Unit>;
