using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;

public sealed class FindDuplicatePatientsQueryHandler : IRequestHandler<FindDuplicatePatientsQuery, IReadOnlyList<DuplicatePatientDto>>
{
    private readonly IPatientRepository _patientRepository;

    public FindDuplicatePatientsQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<IReadOnlyList<DuplicatePatientDto>> Handle(
        FindDuplicatePatientsQuery request,
        CancellationToken cancellationToken)
    {
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
