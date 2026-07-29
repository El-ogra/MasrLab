using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;

public record UpdatePatientDataCommand(
    int Id,
    string Name,
    int AgeYears,
    int AgeMonths,
    int AgeDays,
    Gender Gender,
    string? Phone,
    string? Address,
    string? NationalId,
    string? Notes,
    int? DoctorId,
    int? ReferralEntityId,
    string? DrugAllergy,
    bool Pregnancy,
    bool BloodThinning,
    bool HasDiabetes,
    bool HasHypertension,
    bool HasLiverDisease,
    bool HasJointDisease,
    bool HasRenalFailure,
    bool HasLupus,
    bool HasHeartDisease,
    bool HasThyroidDisorder,
    string? ChronicDiseases
) : IRequest<Unit>;
