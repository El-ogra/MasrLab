using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetGenderStatistics;

public class GetGenderStatisticsQueryHandler : IRequestHandler<GetGenderStatisticsQuery, GenderStatisticsDto>
{
    public Task<GenderStatisticsDto> Handle(GetGenderStatisticsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
