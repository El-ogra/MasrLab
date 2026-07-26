using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetGenderStatistics;

public class GetGenderStatisticsQueryHandler : IRequestHandler<GetGenderStatisticsQuery, StatisticsDto>
{
    public Task<StatisticsDto> Handle(GetGenderStatisticsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
