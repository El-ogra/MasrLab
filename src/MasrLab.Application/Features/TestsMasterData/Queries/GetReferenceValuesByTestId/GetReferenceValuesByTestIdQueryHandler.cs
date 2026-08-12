using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Queries.GetReferenceValuesByTestId;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetReferenceValuesByTestId;

public class GetReferenceValuesByTestIdQueryHandler : IRequestHandler<GetReferenceValuesByTestIdQuery, IReadOnlyList<ReferenceValueDto>>
{
    private readonly IReferenceValueRepository _referenceValueRepository;
    private readonly IMapper _mapper;

    public GetReferenceValuesByTestIdQueryHandler(
        IReferenceValueRepository referenceValueRepository,
        IMapper mapper)
    {
        _referenceValueRepository = referenceValueRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ReferenceValueDto>> Handle(
        GetReferenceValuesByTestIdQuery request,
        CancellationToken cancellationToken)
    {
        var references = await _referenceValueRepository.GetByTestIdAsync(request.TestId, cancellationToken);
        return _mapper.Map<List<ReferenceValueDto>>(references);
    }
}
