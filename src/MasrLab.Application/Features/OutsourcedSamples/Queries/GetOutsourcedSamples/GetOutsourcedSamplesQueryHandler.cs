using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.OutsourcedSamples.Queries.GetOutsourcedSamples;

public class GetOutsourcedSamplesQueryHandler : IRequestHandler<GetOutsourcedSamplesQuery, IReadOnlyList<OutsourcedSampleDto>>
{
    private readonly IRepository<OutsourcedSample> _repository;
    private readonly IMapper _mapper;

    public GetOutsourcedSamplesQueryHandler(IRepository<OutsourcedSample> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OutsourcedSampleDto>> Handle(GetOutsourcedSamplesQuery request, CancellationToken cancellationToken)
    {
        var samples = await _repository.GetAllAsync(cancellationToken);

        var filtered = samples
            .Where(s => s.ReceivedAt >= request.PeriodStart && s.ReceivedAt <= request.PeriodEnd)
            .ToList();

        return filtered.Select(s => _mapper.Map<OutsourcedSampleDto>(s)).ToList();
    }
}
