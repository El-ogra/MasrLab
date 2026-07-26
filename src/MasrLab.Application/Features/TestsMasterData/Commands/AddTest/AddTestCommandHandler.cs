using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddTest;

public class AddTestCommandHandler : IRequestHandler<AddTestCommand, Unit>
{
    public Task<Unit> Handle(AddTestCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
