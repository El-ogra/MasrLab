using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.SampleCollection.Queries.GetPendingSamples;

public class GetPendingSamplesQueryHandler : IRequestHandler<GetPendingSamplesQuery, IReadOnlyList<SampleDto>>
{
    private readonly ISampleRepository _sampleRepository;
    private readonly IMapper _mapper;

    public GetPendingSamplesQueryHandler(ISampleRepository sampleRepository, IMapper mapper)
    {
        _sampleRepository = sampleRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SampleDto>> Handle(GetPendingSamplesQuery request, CancellationToken cancellationToken)
    {
        var pendingSamples = await _sampleRepository.GetPendingAsync(request.PatientVisitId, cancellationToken);

        return pendingSamples.Select(s => _mapper.Map<SampleDto>(s)).ToList();
    }
}
