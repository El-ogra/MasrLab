using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Statistics.Queries.GetMonthlyStatistics;

public class GetMonthlyStatisticsQueryHandler : IRequestHandler<GetMonthlyStatisticsQuery, MonthlyStatisticsDto>
{
    private readonly IStatisticsRepository _statisticsRepository;

    public GetMonthlyStatisticsQueryHandler(IStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<MonthlyStatisticsDto> Handle(GetMonthlyStatisticsQuery request, CancellationToken cancellationToken)
    {
        var domainResult = await _statisticsRepository.GetMonthlyStatisticsAsync(request.PeriodStart, request.PeriodEnd);
        return new MonthlyStatisticsDto(domainResult);
    }
}
