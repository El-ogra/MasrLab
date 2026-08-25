using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.DeleteVisitTransaction;

public record DeleteVisitTransactionCommand(
    int ReceiptId,
    int TransactionId,
    int UserId
) : IRequest;
