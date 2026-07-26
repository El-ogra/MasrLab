using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.SettleOutsourcedAccount;

public class SettleOutsourcedAccountCommandHandler : IRequestHandler<SettleOutsourcedAccountCommand, Unit>
{
    public Task<Unit> Handle(SettleOutsourcedAccountCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
