using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.OutsourcedSamples.Queries.GetOutsourcedSamples;

public class GetOutsourcedSamplesQueryHandler : IRequestHandler<GetOutsourcedSamplesQuery, IReadOnlyList<OutsourcedSampleDto>>
{
    private readonly IOutsourcedSampleRepository _repository;
    private readonly IMapper _mapper;

    public GetOutsourcedSamplesQueryHandler(IOutsourcedSampleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OutsourcedSampleDto>> Handle(GetOutsourcedSamplesQuery request, CancellationToken cancellationToken)
    {
        var samples = await _repository.GetByReceivedDateRangeAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);

        return samples.Select(s => _mapper.Map<OutsourcedSampleDto>(s)).ToList();
    }
}
