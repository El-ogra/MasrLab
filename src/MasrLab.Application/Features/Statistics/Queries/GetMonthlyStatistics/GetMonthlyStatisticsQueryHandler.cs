using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetMonthlyStatistics;

public class GetMonthlyStatisticsQueryHandler : IRequestHandler<GetMonthlyStatisticsQuery, MonthlyStatisticsDto>
{
    public Task<MonthlyStatisticsDto> Handle(GetMonthlyStatisticsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
