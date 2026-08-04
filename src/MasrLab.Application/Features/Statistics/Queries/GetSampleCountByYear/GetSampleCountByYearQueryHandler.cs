using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetSampleCountByYear;

public class GetSampleCountByYearQueryHandler : IRequestHandler<GetSampleCountByYearQuery, SampleCountByYearDto>
{
    public Task<SampleCountByYearDto> Handle(GetSampleCountByYearQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
