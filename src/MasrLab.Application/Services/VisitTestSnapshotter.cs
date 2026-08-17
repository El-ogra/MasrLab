using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

public class VisitTestSnapshotter : IVisitTestSnapshotter
{
    public (VisitTest VisitTest, IReadOnlyList<VisitTestResultItem> ResultItems) CreateVisitTestSnapshot(
        Test test, int visitId, decimal price, bool isOutsourced)
    {
        var activeComponents = test.TestComponents
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToList();

        if (activeComponents.Count == 0)
            throw new BusinessRuleViolationException(
                "Cannot assign a draft (zero-component) test to a visit.");

        var visitTest = new VisitTest(visitId, test.Id, price, isOutsourced)
        {
            TestNameSnapshot = test.Name,
            ReportNameSnapshot = test.ReportName,
            ReceiptNameSnapshot = test.ReceiptName,
            IsCompoundSnapshot = activeComponents.Count > 1
        };

        var resultItems = new List<VisitTestResultItem>();

        foreach (var component in activeComponents)
        {
            var resultItem = new VisitTestResultItem
            {
                VisitTestId = visitTest.Id,
                SourceTestComponentId = component.Id,
                ComponentName = component.Name,
                ComponentUnit = component.Unit,
                DisplayOrder = component.DisplayOrder,
                ResultEntryKind = component.ResultEntryKind
            };
            visitTest.ResultItems.Add(resultItem);
            resultItems.Add(resultItem);
        }

        return (visitTest, resultItems);
    }
}
