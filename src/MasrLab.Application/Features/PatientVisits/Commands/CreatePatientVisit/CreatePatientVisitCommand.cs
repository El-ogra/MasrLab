using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;

public record CreatePatientVisitCommand(
    int PatientId,
    int? DoctorId,
    int? ReferralEntityId,
    bool TakenOutsideLab = false,
    bool SpecimenUrine = false,
    bool SpecimenStool = false,
    bool SpecimenBlood = false,
    bool SpecimenSemen = false,
    bool SpecimenCsf = false
) : IRequest<int>;
