using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public class EnterTestResultCommandHandler : IRequestHandler<EnterTestResultCommand, Unit>
{
    public Task<Unit> Handle(EnterTestResultCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
