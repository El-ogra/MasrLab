using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetTestDemandRate;

public class GetTestDemandRateQueryHandler : IRequestHandler<GetTestDemandRateQuery, TestDemandRateDto>
{
    public Task<TestDemandRateDto> Handle(GetTestDemandRateQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
