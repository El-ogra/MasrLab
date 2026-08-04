using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCaseUserTracking;

public class GetCaseUserTrackingQueryHandler : IRequestHandler<GetCaseUserTrackingQuery, IReadOnlyList<CaseUserTrackingDto>>
{
    public Task<IReadOnlyList<CaseUserTrackingDto>> Handle(GetCaseUserTrackingQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
