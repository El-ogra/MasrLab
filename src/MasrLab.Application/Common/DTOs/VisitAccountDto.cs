using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public sealed record VisitAccountTransactionDto(
    int Id,
    DateTime PaidDate,
    decimal Money,
    string UserName,
    DateTime? EditDate,
    VisitTransactionType Type,
    string ColorCode,
    bool IsDeleted);

// M2-BR-05/06/07: the account-window figures panel and the color-coded transaction grid.
public sealed record VisitAccountDto(
    int ReceiptId,
    int PatientVisitId,
    decimal TestsTotal,
    decimal DiscountPercent,
    decimal DiscountValue,
    decimal TotalAfterDiscount,
    decimal PreviouslyPaid,
    decimal RemainingForLab,
    decimal RemainingForPatient,
    bool IsSettled,
    IReadOnlyList<VisitAccountTransactionDto> Transactions)
{
    public static VisitAccountDto Create(
        int receiptId,
        int patientVisitId,
        decimal testsTotal,
        decimal discountPercent,
        decimal discountValue,
        decimal totalAfterDiscount,
        decimal previouslyPaid,
        decimal paidNow,
        bool isSettled,
        IEnumerable<VisitAccountTransactionSource> transactions,
        IReadOnlyDictionary<int, string> userNamesById)
    {
        var paidTotal = previouslyPaid + paidNow;
        return new VisitAccountDto(
            ReceiptId: receiptId,
            PatientVisitId: patientVisitId,
            TestsTotal: testsTotal,
            DiscountPercent: discountPercent,
            DiscountValue: discountValue,
            TotalAfterDiscount: totalAfterDiscount,
            PreviouslyPaid: previouslyPaid,
            RemainingForLab: Math.Max(0, totalAfterDiscount - paidTotal),
            RemainingForPatient: Math.Max(0, paidTotal - totalAfterDiscount),
            IsSettled: isSettled,
            Transactions: transactions
                .OrderBy(t => t.Id)
                .Select(t => new VisitAccountTransactionDto(
                    t.Id,
                    t.PaidDate,
                    t.Amount,
                    userNamesById.GetValueOrDefault(t.UserId, $"User {t.UserId}"),
                    t.EditDate,
                    t.Type,
                    ColorCodeFor(t.Type),
                    t.IsDeleted))
                .ToList());
    }

    // OQ-M2-6 color mapping — mirrors the domain attribute on VisitPaymentTransaction.
    public static string ColorCodeFor(VisitTransactionType type) => type switch
    {
        VisitTransactionType.Payment => "Green",
        VisitTransactionType.Refund => "Red",
        VisitTransactionType.ExtraCharge => "Blue",
        _ => "Yellow"
    };
}

public readonly record struct VisitAccountTransactionSource(
    int Id,
    DateTime PaidDate,
    decimal Amount,
    int UserId,
    DateTime? EditDate,
    VisitTransactionType Type,
    bool IsDeleted);
