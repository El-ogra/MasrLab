using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.EditVisitTransaction;

public record EditVisitTransactionCommand(
    int ReceiptId,
    int TransactionId,
    decimal NewAmount,
    int UserId
) : IRequest;
