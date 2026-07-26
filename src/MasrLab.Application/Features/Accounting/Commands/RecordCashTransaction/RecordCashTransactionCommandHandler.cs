using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.RecordCashTransaction;

public class RecordCashTransactionCommandHandler : IRequestHandler<RecordCashTransactionCommand, Unit>
{
    public Task<Unit> Handle(RecordCashTransactionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
