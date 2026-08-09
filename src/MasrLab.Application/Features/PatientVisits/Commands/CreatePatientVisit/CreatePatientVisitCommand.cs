using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;

public record CreatePatientVisitCommand(
    int PatientId,
    int? DoctorId,
    int? ReferralEntityId
) : IRequest<int>;
