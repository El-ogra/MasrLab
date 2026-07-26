using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetPatientCountByPeriod;

public class GetPatientCountByPeriodQueryHandler : IRequestHandler<GetPatientCountByPeriodQuery, StatisticsDto>
{
    public Task<StatisticsDto> Handle(GetPatientCountByPeriodQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
