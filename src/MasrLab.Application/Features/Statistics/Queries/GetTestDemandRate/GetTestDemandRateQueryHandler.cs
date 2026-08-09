using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Statistics.Queries.GetTestDemandRate;

public class GetTestDemandRateQueryHandler : IRequestHandler<GetTestDemandRateQuery, TestDemandRateDto>
{
    private readonly IStatisticsRepository _statisticsRepository;

    public GetTestDemandRateQueryHandler(IStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<TestDemandRateDto> Handle(GetTestDemandRateQuery request, CancellationToken cancellationToken)
    {
        var domainResult = await _statisticsRepository.GetTestDemandRateAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);
        // TestDemandRateDto mirrors the Domain-level statistics DTO via its copy
        // constructor; a cross-layer shape transfer kept manual (documented exception).
        return new TestDemandRateDto(domainResult);
    }
}
