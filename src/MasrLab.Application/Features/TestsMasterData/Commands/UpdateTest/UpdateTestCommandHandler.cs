using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;

public class UpdateTestCommandHandler : IRequestHandler<UpdateTestCommand, Unit>
{
    public Task<Unit> Handle(UpdateTestCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
