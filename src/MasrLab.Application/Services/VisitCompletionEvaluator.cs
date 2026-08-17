using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

public class VisitCompletionEvaluator : IVisitCompletionEvaluator
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly ICultureRepository _cultureRepository;

    public VisitCompletionEvaluator(
        ITestResultRepository testResultRepository,
        ICultureRepository cultureRepository)
    {
        _testResultRepository = testResultRepository;
        _cultureRepository = cultureRepository;
    }

    public async Task EvaluateAsync(PatientVisit visit, CancellationToken ct = default)
    {
        if (visit.Status != VisitStatus.Registered)
            return;

        foreach (var visitTest in visit.VisitTests)
        {
            foreach (var resultItem in visitTest.ResultItems)
            {
                var isComplete = await IsResultItemCompleteAsync(resultItem, ct);
                if (!isComplete)
                    return;
            }
        }

        if (visit.VisitTests.Any())
            visit.EnterAllResults();
    }

    private async Task<bool> IsResultItemCompleteAsync(VisitTestResultItem resultItem, CancellationToken ct)
    {
        if (resultItem.ResultEntryKind == ResultEntryKind.CultureDetail)
        {
            var culture = await _cultureRepository.GetByVisitTestResultItemIdAsync(resultItem.Id, ct);
            return culture is not null &&
                   (culture.Status == CultureStatus.Recorded || culture.Status == CultureStatus.WithSensitivity);
        }

        var results = await _testResultRepository.GetByVisitTestResultItemIdAsync(resultItem.Id, ct);
        return results.Any(r => !r.IsDeleted);
    }
}
