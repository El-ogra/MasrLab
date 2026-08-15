using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetResultTree;

public class GetResultTreeQueryHandler : IRequestHandler<GetResultTreeQuery, IReadOnlyList<VisitTestWithComponentsDto>>
{
    private readonly IVisitRepository _visitRepository;
    private readonly ITestResultRepository _testResultRepository;
    private readonly ICultureRepository _cultureRepository;

    public GetResultTreeQueryHandler(
        IVisitRepository visitRepository,
        ITestResultRepository testResultRepository,
        ICultureRepository cultureRepository)
    {
        _visitRepository = visitRepository;
        _testResultRepository = testResultRepository;
        _cultureRepository = cultureRepository;
    }

    public async Task<IReadOnlyList<VisitTestWithComponentsDto>> Handle(GetResultTreeQuery request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken);
        if (visit is null) return new List<VisitTestWithComponentsDto>();

        var result = new List<VisitTestWithComponentsDto>();

        foreach (var visitTest in visit.VisitTests.OrderBy(vt => vt.Id))
        {
            var components = new List<VisitTestResultItemWithResultDto>();

            foreach (var resultItem in visitTest.ResultItems.OrderBy(ri => ri.DisplayOrder))
            {
                TestResultDto? testResultDto = null;
                CultureResultDto? cultureResultDto = null;

                if (resultItem.ResultEntryKind == ResultEntryKind.CultureDetail)
                {
                    var culture = await _cultureRepository.GetByVisitTestResultItemIdAsync(resultItem.Id, cancellationToken);
                    if (culture is not null)
                    {
                        cultureResultDto = new CultureResultDto
                        {
                            Id = culture.Id,
                            SampleType = culture.SampleType,
                            OrganismA = culture.OrganismA,
                            OrganismB = culture.OrganismB,
                            OrganismC = culture.OrganismC,
                            CultureCondition = culture.CultureCondition,
                            ColonyCount = culture.ColonyCount
                        };
                    }
                }
                else
                {
                    var testResult = await _testResultRepository.GetByVisitTestResultItemIdAsync(resultItem.Id, cancellationToken);
                    var firstResult = testResult?.FirstOrDefault();
                    if (firstResult is not null)
                    {
                        testResultDto = new TestResultDto
                        {
                            Id = firstResult.Id,
                            VisitTestResultItemId = firstResult.VisitTestResultItemId,
                            Value = firstResult.Value,
                            Unit = firstResult.Unit,
                            ReferenceRange = firstResult.ReferenceRange,
                            Status = firstResult.Status,
                            EnteredByUserId = firstResult.EnteredByUserId,
                            EnteredAt = firstResult.EnteredAt,
                            PrintedByUserId = firstResult.PrintedByUserId,
                            PrintedAt = firstResult.PrintedAt,
                            PrintCount = firstResult.PrintCount
                        };
                    }
                }

                components.Add(new VisitTestResultItemWithResultDto
                {
                    Id = resultItem.Id,
                    ComponentName = resultItem.ComponentName,
                    ComponentUnit = resultItem.ComponentUnit,
                    DisplayOrder = resultItem.DisplayOrder,
                    ResultEntryKind = resultItem.ResultEntryKind.ToString(),
                    TestResult = testResultDto,
                    CultureResult = cultureResultDto
                });
            }

            result.Add(new VisitTestWithComponentsDto
            {
                VisitTestId = visitTest.Id,
                TestId = visitTest.TestId,
                TestNameSnapshot = visitTest.TestNameSnapshot,
                ReportNameSnapshot = visitTest.ReportNameSnapshot,
                IsCompoundSnapshot = visitTest.IsCompoundSnapshot,
                Price = visitTest.Price,
                Components = components
            });
        }

        return result;
    }
}
