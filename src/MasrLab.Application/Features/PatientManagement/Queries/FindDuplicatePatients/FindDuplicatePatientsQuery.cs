using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;

public sealed record FindDuplicatePatientsQuery(
    string Name,
    string? NationalId,
    string? Phone) : IRequest<IReadOnlyList<DuplicatePatientDto>>;
