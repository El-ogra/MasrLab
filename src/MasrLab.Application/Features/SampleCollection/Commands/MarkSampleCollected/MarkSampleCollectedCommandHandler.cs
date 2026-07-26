using MediatR;

namespace MasrLab.Application.Features.SampleCollection.Commands.MarkSampleCollected;

public class MarkSampleCollectedCommandHandler : IRequestHandler<MarkSampleCollectedCommand, Unit>
{
    public Task<Unit> Handle(MarkSampleCollectedCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
