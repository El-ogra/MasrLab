using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PatientHistory.Queries.GetPatientHistory;

public class GetPatientHistoryQueryHandler : IRequestHandler<GetPatientHistoryQuery, IReadOnlyList<PatientHistoryDto>>
{
    public Task<IReadOnlyList<PatientHistoryDto>> Handle(GetPatientHistoryQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
