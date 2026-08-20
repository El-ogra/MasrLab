using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetExternalLabCandidates;

public class GetExternalLabCandidatesQueryHandler : IRequestHandler<GetExternalLabCandidatesQuery, IReadOnlyList<ReferralEntityDto>>
{
    private readonly IReferralEntityRepository _repository;
    private readonly IMapper _mapper;

    public GetExternalLabCandidatesQueryHandler(IReferralEntityRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ReferralEntityDto>> Handle(GetExternalLabCandidatesQuery request, CancellationToken cancellationToken)
    {
        var candidates = await _repository.GetExternalLabCandidatesAsync(cancellationToken);
        return candidates.Select(e => _mapper.Map<ReferralEntityDto>(e)).ToList();
    }
}
