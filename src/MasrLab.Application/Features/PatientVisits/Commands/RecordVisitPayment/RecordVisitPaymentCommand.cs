using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.RecordVisitPayment;

public record RecordVisitPaymentCommand(
    int ReceiptId,
    decimal Amount,
    int UserId
) : IRequest<int>;
