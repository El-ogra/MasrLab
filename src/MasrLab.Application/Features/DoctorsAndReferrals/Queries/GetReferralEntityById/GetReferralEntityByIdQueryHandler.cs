using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetReferralEntityById;

public class GetReferralEntityByIdQueryHandler : IRequestHandler<GetReferralEntityByIdQuery, ReferralEntityDto?>
{
    private readonly IReferralEntityRepository _repository;
    private readonly IMapper _mapper;

    public GetReferralEntityByIdQueryHandler(IReferralEntityRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ReferralEntityDto?> Handle(GetReferralEntityByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdWithPriceListAsync(request.Id, cancellationToken);
        if (entity is null) return null;
        return _mapper.Map<ReferralEntityDto>(entity);
    }
}
