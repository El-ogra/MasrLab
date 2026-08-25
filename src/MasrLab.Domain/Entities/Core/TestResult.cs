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

    public string? Comment { get; private set; }
    public string? AutoCommentSnapshot { get; private set; }
    public bool ReprintRequired { get; private set; }
    public bool IsStatusOverridden { get; private set; }

    // OQ-M4-4: manual H/L override on top of the automatic M10 engine.
    // A forced value flags the result; null clears the override so the engine re-derives it.
    public void OverrideStatus(ResultStatus? forcedStatus, int userId)
    {
        if (userId <= 0)
            throw new BusinessRuleViolationException("A valid user is required to override a status.");

        if (forcedStatus is null)
        {
            if (!IsStatusOverridden)
                throw new BusinessRuleViolationException("There is no status override to clear.");
            IsStatusOverridden = false;
            EditedByUserId = userId;
            EditedAt = DateTime.UtcNow;
            return;
        }

        Status = forcedStatus.Value;
        IsStatusOverridden = true;
        EditedByUserId = userId;
        EditedAt = DateTime.UtcNow;
    }

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

    public void Edit(string newValue, string? newComment, int editedByUserId)
    {
        if (string.IsNullOrWhiteSpace(newValue))
            throw new BusinessRuleViolationException("Test result value cannot be empty.");

        var oldValue = Value;
        var oldComment = Comment;
        var commentChanged = !string.Equals(oldComment, newComment, StringComparison.Ordinal);

        Value = newValue;

        if (newComment != null || commentChanged)
            SetComment(newComment);

        EditedByUserId = editedByUserId;
        EditedAt = DateTime.UtcNow;

        var changeType = (oldValue != newValue, commentChanged) switch
        {
            (true, true) => ResultEditChangeType.ValueAndComment,
            (true, false) => ResultEditChangeType.ValueOnly,
            (false, true) => ResultEditChangeType.CommentOnly,
            (false, false) => throw new BusinessRuleViolationException("No changes detected.")
        };

        AddDomainEvent(new TestResultEdited(
            Id, VisitTestResultItemId, oldValue, newValue,
            oldComment, newComment, changeType, editedByUserId));
    }

    public void EditComment(string? newComment, int editedByUserId)
    {
        var oldComment = Comment;
        var commentChanged = !string.Equals(oldComment, newComment, StringComparison.Ordinal);

        if (!commentChanged)
            throw new BusinessRuleViolationException("No changes detected.");

        EditedByUserId = editedByUserId;
        EditedAt = DateTime.UtcNow;

        SetComment(newComment);

        AddDomainEvent(new TestResultEdited(
            Id, VisitTestResultItemId, Value, Value,
            oldComment, newComment, ResultEditChangeType.CommentOnly, editedByUserId));
    }

    public void ReapplyReference(
        string newReferenceRange,
        ResultStatus newStatus,
        string? newAutoComment,
        int appliedByUserId)
    {
        if (appliedByUserId <= 0)
            throw new BusinessRuleViolationException("Applied-by user is required.");

        var oldComment = Comment;
        var shouldReplaceComment = Comment is null ||
            string.Equals(Comment, AutoCommentSnapshot, StringComparison.Ordinal);

        ReferenceRange = newReferenceRange;
        Status = newStatus;

        if (shouldReplaceComment)
            SetAutomaticComment(newAutoComment);

        EditedByUserId = appliedByUserId;
        EditedAt = DateTime.UtcNow;

        AddDomainEvent(new TestResultReferenceReapplied(
            Id, oldComment, Comment, appliedByUserId));
    }

    public void SetComment(string? comment)
    {
        if (comment is not null && comment.Length > 1000)
            throw new BusinessRuleViolationException("Comment cannot exceed 1000 characters.");
        Comment = comment;
    }

    public void SetAutomaticComment(string? comment)
    {
        if (comment is not null && comment.Length > 1000)
            throw new BusinessRuleViolationException("Comment cannot exceed 1000 characters.");
        Comment = comment;
        AutoCommentSnapshot = comment;
    }

    public void MarkReprintRequired()
    {
        ReprintRequired = true;
    }

    public void MarkPrinted(int printedByUserId)
    {
        PrintCount++;
        PrintedByUserId = printedByUserId;
        PrintedAt = DateTime.UtcNow;
        ReprintRequired = false;
    }
}
