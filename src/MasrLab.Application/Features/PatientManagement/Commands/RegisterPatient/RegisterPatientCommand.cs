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
    int? ReferralEntityId
) : IRequest<Unit>;
