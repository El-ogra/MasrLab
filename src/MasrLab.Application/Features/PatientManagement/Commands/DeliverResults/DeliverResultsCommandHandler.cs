using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.DeliverResults;

public class DeliverResultsCommandHandler : IRequestHandler<DeliverResultsCommand, Unit>
{
    public Task<Unit> Handle(DeliverResultsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
