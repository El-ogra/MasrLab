using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.MarkTestAsOutsourced;

public class MarkTestAsOutsourcedCommandHandler : IRequestHandler<MarkTestAsOutsourcedCommand, Unit>
{
    public Task<Unit> Handle(MarkTestAsOutsourcedCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
