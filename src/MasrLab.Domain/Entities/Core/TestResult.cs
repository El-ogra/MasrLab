using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class TestResult : BaseEntity
{
    public int VisitTestResultItemId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public ResultStatus Status { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; }

    public string? OverrideReason { get; set; }
    public int? PrintedByUserId { get; set; }
    public DateTime? PrintedAt { get; set; }
    public int PrintCount { get; set; }
    public int EditedByUserId { get; set; }
    public DateTime? EditedAt { get; set; }

    public static TestResult Enter(int visitTestResultItemId, string value, int enteredByUserId)
    {
        if (visitTestResultItemId <= 0)
            throw new BusinessRuleViolationException("TestResult requires a valid VisitTestResultItemId.");
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleViolationException("Test result value cannot be empty.");
        var result = new TestResult
        {
            VisitTestResultItemId = visitTestResultItemId,
            Value = value,
            EnteredByUserId = enteredByUserId,
            EnteredAt = DateTime.UtcNow
        };
        result.AddDomainEvent(new TestResultEntered(result.Id, visitTestResultItemId, value, enteredByUserId));
        return result;
    }

    public void Edit(string newValue, int editedByUserId)
    {
        if (string.IsNullOrWhiteSpace(newValue))
            throw new BusinessRuleViolationException("Test result value cannot be empty.");
        var oldValue = Value;
        Value = newValue;
        EditedByUserId = editedByUserId;
        EditedAt = DateTime.UtcNow;
        AddDomainEvent(new TestResultEdited(Id, VisitTestResultItemId, oldValue, newValue, editedByUserId));
    }
}
