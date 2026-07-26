using MediatR;
using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetMonthlyStatistics;

public class GetMonthlyStatisticsQueryHandler : IRequestHandler<GetMonthlyStatisticsQuery, StatisticsDto>
{
    public Task<StatisticsDto> Handle(GetMonthlyStatisticsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
