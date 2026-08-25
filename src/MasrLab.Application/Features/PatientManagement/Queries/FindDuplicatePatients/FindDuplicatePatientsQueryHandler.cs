using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;

public sealed class FindDuplicatePatientsQueryHandler : IRequestHandler<FindDuplicatePatientsQuery, IReadOnlyList<DuplicatePatientDto>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly ICurrentUserService _currentUserService;

    public FindDuplicatePatientsQueryHandler(
        IPatientRepository patientRepository,
        ICurrentUserService currentUserService)
    {
        _patientRepository = patientRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<DuplicatePatientDto>> Handle(
        FindDuplicatePatientsQuery request,
        CancellationToken cancellationToken)
    {
        _ = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated.");

        var patients = await _patientRepository.FindProbableDuplicatesAsync(
            request.Name,
            request.NationalId,
            request.Phone,
            cancellationToken);

        return patients
            .Select(patient => new DuplicatePatientDto(
                patient.Id,
                patient.Name,
                patient.LabId,
                patient.NationalId,
                patient.Phone?.Value))
            .ToList();
    }
}
