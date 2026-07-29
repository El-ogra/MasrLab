using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public record RegisterPatientCommand(
    string Name,
    int AgeYears,
    int AgeMonths,
    int AgeDays,
    Gender Gender,
    string? Phone,
    string? Address,
    string? NationalId,
    string? Notes,
    string LabId,
    int? DoctorId,
    int? ReferralEntityId,
    AccountType AccountType,
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
