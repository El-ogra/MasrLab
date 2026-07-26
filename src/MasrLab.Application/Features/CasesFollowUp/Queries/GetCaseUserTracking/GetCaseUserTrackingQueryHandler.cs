using MediatR;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCaseUserTracking;

public class GetCaseUserTrackingQueryHandler : IRequestHandler<GetCaseUserTrackingQuery, IReadOnlyList<object>>
{
    public Task<IReadOnlyList<object>> Handle(GetCaseUserTrackingQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
