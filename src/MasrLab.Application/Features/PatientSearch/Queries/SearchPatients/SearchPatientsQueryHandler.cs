using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PatientSearch.Queries.SearchPatients;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientSearch.Queries.SearchPatients;

public class SearchPatientsQueryHandler : IRequestHandler<SearchPatientsQuery, IReadOnlyList<PatientDto>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IMapper _mapper;

    public SearchPatientsQueryHandler(IPatientRepository patientRepository, IMapper mapper)
    {
        _patientRepository = patientRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PatientDto>> Handle(SearchPatientsQuery request, CancellationToken cancellationToken)
    {
        var patients = await _patientRepository.SearchByNameAsync(request.SearchTerm, cancellationToken);

        return patients.Select(p => _mapper.Map<PatientDto>(p)).ToList();
    }
}
