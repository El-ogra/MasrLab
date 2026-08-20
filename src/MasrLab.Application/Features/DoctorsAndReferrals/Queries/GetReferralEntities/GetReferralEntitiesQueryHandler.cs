using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetReferralEntities;

public class GetReferralEntitiesQueryHandler : IRequestHandler<GetReferralEntitiesQuery, IReadOnlyList<ReferralEntityDto>>
{
    private readonly IReferralEntityRepository _repository;
    private readonly IMapper _mapper;

    public GetReferralEntitiesQueryHandler(IReferralEntityRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ReferralEntityDto>> Handle(GetReferralEntitiesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllWithPriceListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<ReferralEntityDto>(e)).ToList();
    }
}
