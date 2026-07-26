using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.EnterCultureResult;

public class EnterCultureResultCommandHandler : IRequestHandler<EnterCultureResultCommand, Unit>
{
    public Task<Unit> Handle(EnterCultureResultCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
