using MediatR;
using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetSampleCountByYear;

public class GetSampleCountByYearQueryHandler : IRequestHandler<GetSampleCountByYearQuery, StatisticsDto>
{
    public Task<StatisticsDto> Handle(GetSampleCountByYearQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
