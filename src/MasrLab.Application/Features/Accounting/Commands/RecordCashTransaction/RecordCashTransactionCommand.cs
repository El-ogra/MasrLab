using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.Accounting.Commands.RecordCashTransaction;

public record RecordCashTransactionCommand(TransactionType Type, decimal Amount, int EntityId, int UserId, DateTime TransactionDate) : IRequest<Unit>;
