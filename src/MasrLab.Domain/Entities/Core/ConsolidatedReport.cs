using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

// M4-BR-10/11: consolidated report composition with user ordering.
// OQ-M4-14: un-entered tests are allowed as members — they render blank with a placeholder.
public class ConsolidatedReport : BaseEntity
{
    public int PatientVisitId { get; private set; }

    // M4-BR-12: toggle for grouping rows under their test-group sub-titles.
    public bool PrintGroupSubtitles { get; private set; }
    public string? Comment { get; private set; }

    public DateTime? PrintedAt { get; private set; }
    public int? PrintedByUserId { get; private set; }
    public int PrintCount { get; private set; }

    private readonly List<ConsolidatedReportItem> _items = new();
    public IReadOnlyList<ConsolidatedReportItem> Items => _items.AsReadOnly();

    private ConsolidatedReport()
    {
    }

    public static ConsolidatedReport Create(int patientVisitId, bool printGroupSubtitles = true)
    {
        if (patientVisitId <= 0)
            throw new BusinessRuleViolationException("Consolidated report requires a valid patient visit.");

        return new ConsolidatedReport
        {
            PatientVisitId = patientVisitId,
            PrintGroupSubtitles = printGroupSubtitles
        };
    }

    public ConsolidatedReportItem AddItem(int visitTestId)
    {
        if (visitTestId <= 0)
            throw new BusinessRuleViolationException("A consolidated report item requires a valid visit test.");
        if (_items.Any(i => i.VisitTestId == visitTestId))
            throw new BusinessRuleViolationException("Each test may appear only once in a consolidated report.");

        var item = ConsolidatedReportItem.Create(Id, visitTestId, _items.Count + 1);
        _items.Add(item);
        return item;
    }

    public void RemoveItem(int visitTestId)
    {
        var item = _items.FirstOrDefault(i => i.VisitTestId == visitTestId);
        if (item is null)
            throw new BusinessRuleViolationException("Visit test is not part of this consolidated report.");
        _items.Remove(item);
        RenumberFrom(item.DisplayOrder - 1);
    }

    // Reorder mechanics: moving swaps positions and keeps DisplayOrder dense (1..n).
    public void MoveUp(int visitTestId) => Move(visitTestId, -1);
    public void MoveDown(int visitTestId) => Move(visitTestId, +1);

    private void Move(int visitTestId, int delta)
    {
        var index = _items.FindIndex(i => i.VisitTestId == visitTestId);
        if (index < 0)
            throw new BusinessRuleViolationException("Visit test is not part of this consolidated report.");
        var target = index + delta;
        if (target < 0 || target >= _items.Count)
            return; // already at the edge — no-op.

        (_items[index], _items[target]) = (_items[target], _items[index]);
        RenumberFrom(Math.Min(index, target));
    }

    private void RenumberFrom(int startIndex)
    {
        for (var i = startIndex; i < _items.Count; i++)
            _items[i].SetDisplayOrder(i + 1);
    }

    public void SetComment(string? comment) =>
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

    public void SetPrintGroupSubtitles(bool enabled) => PrintGroupSubtitles = enabled;

    public void MarkPrinted(int printedByUserId)
    {
        PrintCount++;
        PrintedAt = DateTime.UtcNow;
        PrintedByUserId = printedByUserId;
    }
}
