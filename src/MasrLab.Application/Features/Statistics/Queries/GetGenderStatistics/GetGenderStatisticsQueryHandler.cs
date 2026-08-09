using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Statistics.Queries.GetGenderStatistics;

public class GetGenderStatisticsQueryHandler : IRequestHandler<GetGenderStatisticsQuery, GenderStatisticsDto>
{
    private readonly IStatisticsRepository _statisticsRepository;

    public GetGenderStatisticsQueryHandler(IStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<GenderStatisticsDto> Handle(GetGenderStatisticsQuery request, CancellationToken cancellationToken)
    {
        var domainResult = await _statisticsRepository.GetGenderStatisticsAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);
        // GenderStatisticsDto mirrors the Domain-level statistics DTO via its copy
        // constructor; a cross-layer shape transfer kept manual (documented exception).
        return new GenderStatisticsDto(domainResult);
    }
}
