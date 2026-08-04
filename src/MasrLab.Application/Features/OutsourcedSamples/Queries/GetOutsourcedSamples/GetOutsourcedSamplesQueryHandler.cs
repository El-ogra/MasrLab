using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.OutsourcedSamples.Queries.GetOutsourcedSamples;

public class GetOutsourcedSamplesQueryHandler : IRequestHandler<GetOutsourcedSamplesQuery, IReadOnlyList<OutsourcedSampleDto>>
{
    private readonly IRepository<OutsourcedSample> _repository;

    public GetOutsourcedSamplesQueryHandler(IRepository<OutsourcedSample> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<OutsourcedSampleDto>> Handle(GetOutsourcedSamplesQuery request, CancellationToken cancellationToken)
    {
        var samples = await _repository.GetAllAsync();

        var filtered = samples
            .Where(s => s.ReceivedAt >= request.PeriodStart && s.ReceivedAt <= request.PeriodEnd)
            .ToList();

        return filtered.Select(s => new OutsourcedSampleDto
        {
            Id = s.Id,
            PatientVisitId = s.PatientVisitId,
            TestId = s.TestId,
            ExternalLabId = s.ExternalLabId,
            CostPrice = s.CostPrice,
            PatientPrice = s.PatientPrice,
            SettlementStatus = s.SettlementStatus,
            ReceivedAt = s.ReceivedAt
        }).ToList();
    }
}
