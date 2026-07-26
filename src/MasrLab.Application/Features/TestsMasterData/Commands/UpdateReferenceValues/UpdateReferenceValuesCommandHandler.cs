using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValues;

public class UpdateReferenceValuesCommandHandler : IRequestHandler<UpdateReferenceValuesCommand, Unit>
{
    public Task<Unit> Handle(UpdateReferenceValuesCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
