using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetTestResultForVisit;

public class GetTestResultForVisitQueryHandler : IRequestHandler<GetTestResultForVisitQuery, IReadOnlyList<TestResultDto>>
{
    public Task<IReadOnlyList<TestResultDto>> Handle(GetTestResultForVisitQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
