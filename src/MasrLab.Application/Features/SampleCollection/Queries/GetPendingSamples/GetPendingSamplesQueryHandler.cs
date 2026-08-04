using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.SampleCollection.Queries.GetPendingSamples;

public class GetPendingSamplesQueryHandler : IRequestHandler<GetPendingSamplesQuery, IReadOnlyList<SampleDto>>
{
    private readonly IRepository<Sample> _sampleRepository;

    public GetPendingSamplesQueryHandler(IRepository<Sample> sampleRepository)
    {
        _sampleRepository = sampleRepository;
    }

    public async Task<IReadOnlyList<SampleDto>> Handle(GetPendingSamplesQuery request, CancellationToken cancellationToken)
    {
        var allSamples = await _sampleRepository.GetAllAsync();

        var pendingSamples = allSamples.Where(s => s.CollectionStatus == SampleStatus.NotCollected);

        if (request.PatientVisitId.HasValue)
        {
            pendingSamples = pendingSamples.Where(s => s.PatientVisitId == request.PatientVisitId.Value);
        }

        return pendingSamples.Select(s => new SampleDto
        {
            Id = s.Id,
            PatientVisitId = s.PatientVisitId,
            TestId = s.TestId,
            SampleType = s.SampleType,
            Barcode = s.Barcode,
            CollectionStatus = s.CollectionStatus
        }).ToList();
    }
}
