using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Financial;

// M2-BR-07: one immutable row per Pay / Refund / Extra-charge / Edit action.
// Original rows are never overwritten; edits append an Adjustment row and stamp EditDate.
public class VisitPaymentTransaction : BaseEntity
{
    public int ReceiptId { get; private set; }
    public VisitTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaidDate { get; private set; }
    public int UserId { get; private set; }
    public DateTime? EditDate { get; private set; }

    private VisitPaymentTransaction()
    {
    }

    public static VisitPaymentTransaction Create(
        int receiptId,
        VisitTransactionType type,
        decimal amount,
        int userId,
        DateTime? paidDateUtc = null)
    {
        if (receiptId < 0)
            throw new BusinessRuleViolationException("Visit payment transaction requires a valid ReceiptId.");
        if (amount <= 0)
            throw new BusinessRuleViolationException("Visit payment transaction amount must be greater than zero.");
        if (userId < 0)
            throw new BusinessRuleViolationException("Visit payment transaction requires a valid UserId.");

        return new VisitPaymentTransaction
        {
            ReceiptId = receiptId,
            Type = type,
            Amount = amount,
            PaidDate = paidDateUtc ?? DateTime.UtcNow,
            UserId = userId
        };
    }

    // OQ-M2-6: color is a visual-only, derived display attribute of the type.
    public string ColorCode => Type switch
    {
        VisitTransactionType.Payment => "Green",
        VisitTransactionType.Refund => "Red",
        VisitTransactionType.ExtraCharge => "Blue",
        VisitTransactionType.Adjustment => "Yellow",
        _ => "Yellow"
    };

    internal void MarkEdited(DateTime editDateUtc)
    {
        EditDate = editDateUtc;
    }
}
