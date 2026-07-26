using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.CreateAccountTypeDrawer;

public class CreateAccountTypeDrawerCommandHandler : IRequestHandler<CreateAccountTypeDrawerCommand, Unit>
{
    public Task<Unit> Handle(CreateAccountTypeDrawerCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
