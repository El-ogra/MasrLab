using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.RecordVisitRefund;

public record RecordVisitRefundCommand(
    int ReceiptId,
    decimal Amount,
    int UserId
) : IRequest<int>;
