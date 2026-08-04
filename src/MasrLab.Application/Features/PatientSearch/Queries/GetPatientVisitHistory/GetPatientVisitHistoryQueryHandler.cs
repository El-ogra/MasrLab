using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PatientSearch.Queries.GetPatientVisitHistory;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientSearch.Queries.GetPatientVisitHistory;

public class GetPatientVisitHistoryQueryHandler : IRequestHandler<GetPatientVisitHistoryQuery, IReadOnlyList<VisitDto>>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IMapper _mapper;

    public GetPatientVisitHistoryQueryHandler(IVisitRepository visitRepository, IMapper mapper)
    {
        _visitRepository = visitRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VisitDto>> Handle(GetPatientVisitHistoryQuery request, CancellationToken cancellationToken)
    {
        var visits = await _visitRepository.GetByPatientIdAsync(request.PatientId);

        return visits.Select(v => _mapper.Map<VisitDto>(v)).ToList();
    }
}
