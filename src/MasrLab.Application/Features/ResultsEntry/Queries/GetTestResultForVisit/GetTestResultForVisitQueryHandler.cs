using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetTestResultForVisit;

public class GetTestResultForVisitQueryHandler : IRequestHandler<GetTestResultForVisitQuery, IReadOnlyList<TestResultDto>>
{
    private readonly ITestResultRepository _testResultRepository;

    public GetTestResultForVisitQueryHandler(ITestResultRepository testResultRepository)
    {
        _testResultRepository = testResultRepository;
    }

    public async Task<IReadOnlyList<TestResultDto>> Handle(GetTestResultForVisitQuery request, CancellationToken cancellationToken)
    {
        var results = await _testResultRepository.GetByVisitTestIdAsync(request.VisitTestId);

        return results.Select(r => new TestResultDto
        {
            Id = r.Id,
            VisitTestId = r.VisitTestId,
            Value = r.Value,
            Unit = r.Unit,
            ReferenceRange = r.ReferenceRange,
            Status = r.Status,
            EnteredByUserId = r.EnteredByUserId,
            EnteredAt = r.EnteredAt,
            PrintedByUserId = r.PrintedByUserId,
            PrintedAt = r.PrintedAt,
            PrintCount = r.PrintCount
        }).ToList();
    }
}
