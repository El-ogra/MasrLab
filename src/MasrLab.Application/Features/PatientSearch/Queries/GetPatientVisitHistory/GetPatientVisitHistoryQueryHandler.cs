using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PatientSearch.Queries.GetPatientVisitHistory;

public class GetPatientVisitHistoryQueryHandler : IRequestHandler<GetPatientVisitHistoryQuery, IReadOnlyList<VisitDto>>
{
    public Task<IReadOnlyList<VisitDto>> Handle(GetPatientVisitHistoryQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
