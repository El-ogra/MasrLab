using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetTestDemandRate;

public class GetTestDemandRateQueryHandler : IRequestHandler<GetTestDemandRateQuery, StatisticsDto>
{
    public Task<StatisticsDto> Handle(GetTestDemandRateQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
