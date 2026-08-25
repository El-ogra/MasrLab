using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class ConsolidatedReportItem : BaseEntity
{
    public int ConsolidatedReportId { get; private set; }
    public int VisitTestId { get; private set; }
    public int DisplayOrder { get; private set; }

    private ConsolidatedReportItem()
    {
    }

    internal static ConsolidatedReportItem Create(int consolidatedReportId, int visitTestId, int displayOrder)
    {
        if (consolidatedReportId < 0)
            throw new BusinessRuleViolationException("A consolidated report item requires a valid report.");
        if (visitTestId <= 0)
            throw new BusinessRuleViolationException("A consolidated report item requires a valid visit test.");
        if (displayOrder <= 0)
            throw new BusinessRuleViolationException("Display order must be positive.");

        return new ConsolidatedReportItem
        {
            ConsolidatedReportId = consolidatedReportId,
            VisitTestId = visitTestId,
            DisplayOrder = displayOrder
        };
    }

    internal void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder <= 0)
            throw new BusinessRuleViolationException("Display order must be positive.");
        DisplayOrder = displayOrder;
    }
}
