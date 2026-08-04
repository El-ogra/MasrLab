using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCasesByPeriod;

public class GetCasesByPeriodQueryHandler : IRequestHandler<GetCasesByPeriodQuery, IReadOnlyList<VisitDto>>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IMapper _mapper;

    public GetCasesByPeriodQueryHandler(IVisitRepository visitRepository, IMapper mapper)
    {
        _visitRepository = visitRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VisitDto>> Handle(GetCasesByPeriodQuery request, CancellationToken cancellationToken)
    {
        var visits = await _visitRepository.GetByDateRangeAsync(request.PeriodStart, request.PeriodEnd);

        return visits.Select(v => _mapper.Map<VisitDto>(v)).ToList();
    }
}
