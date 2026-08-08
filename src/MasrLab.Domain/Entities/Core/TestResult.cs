using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class TestResult : BaseEntity
{
    public int VisitTestId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public ResultStatus Status { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; }

    /// <summary>
    /// مبرر التجاوز الموثق عند إدخال نتيجة لاختبار عينته غير مجمّعة (مسار استثنائي).
    /// audit trail: يُحفظ مع النتيجة نفسها ليبقى قابلاً للتتبع لاحقاً؛ يبقى null في الإدخال العادي.
    /// </summary>
    public string? OverrideReason { get; set; }
    public int? PrintedByUserId { get; set; }
    public DateTime? PrintedAt { get; set; }
    public int PrintCount { get; set; }
    public int EditedByUserId { get; set; }
    public DateTime? EditedAt { get; set; }

    public static TestResult Enter(int visitTestId, string value, int enteredByUserId)
    {
        if (visitTestId <= 0)
            throw new BusinessRuleViolationException("TestResult requires a valid VisitTestId.");
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleViolationException("Test result value cannot be empty.");
        var result = new TestResult
        {
            VisitTestId = visitTestId,
            Value = value,
            EnteredByUserId = enteredByUserId,
            EnteredAt = DateTime.UtcNow
        };
        result.AddDomainEvent(new TestResultEntered(result.Id, visitTestId, value, enteredByUserId));
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
        AddDomainEvent(new TestResultEdited(Id, VisitTestId, oldValue, newValue, editedByUserId));
    }
}
