using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.SampleCollection.Queries.GetPendingSamples;

public class GetPendingSamplesQueryHandler : IRequestHandler<GetPendingSamplesQuery, IReadOnlyList<SampleDto>>
{
    private readonly IRepository<Sample> _sampleRepository;
    private readonly IMapper _mapper;

    public GetPendingSamplesQueryHandler(IRepository<Sample> sampleRepository, IMapper mapper)
    {
        _sampleRepository = sampleRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SampleDto>> Handle(GetPendingSamplesQuery request, CancellationToken cancellationToken)
    {
        var allSamples = await _sampleRepository.GetAllAsync(cancellationToken);

        var pendingSamples = allSamples.Where(s => s.CollectionStatus == SampleStatus.NotCollected);

        if (request.PatientVisitId.HasValue)
        {
            pendingSamples = pendingSamples.Where(s => s.PatientVisitId == request.PatientVisitId.Value);
        }

        return pendingSamples.Select(s => _mapper.Map<SampleDto>(s)).ToList();
    }
}
