using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.CreatePeriodDrawer;

public class CreatePeriodDrawerCommandHandler : IRequestHandler<CreatePeriodDrawerCommand, Unit>
{
    public Task<Unit> Handle(CreatePeriodDrawerCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
