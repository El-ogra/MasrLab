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
    int DoctorId,
    int ReferralEntityId,
    string? DrugAllergy,
    bool Pregnancy,
    bool BloodThinning,
    string? ChronicDiseases
) : IRequest<Unit>;
