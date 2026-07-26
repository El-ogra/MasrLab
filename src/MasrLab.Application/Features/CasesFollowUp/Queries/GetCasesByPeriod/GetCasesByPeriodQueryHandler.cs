using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCasesByPeriod;

public class GetCasesByPeriodQueryHandler : IRequestHandler<GetCasesByPeriodQuery, IReadOnlyList<VisitDto>>
{
    public Task<IReadOnlyList<VisitDto>> Handle(GetCasesByPeriodQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
