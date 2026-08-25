using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.IssueReceipt;

public record IssueReceiptCommand(
    int PatientVisitId,
    decimal Discount,
    decimal PaidNow,
    int ReceivedByUserId,
    int? CashAccountId,
    decimal DiscountPercent = 0
) : IRequest<int>;
