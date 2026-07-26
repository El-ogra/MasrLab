using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.RecordCashTransaction;

public record RecordCashTransactionCommand : IRequest<Unit>;
