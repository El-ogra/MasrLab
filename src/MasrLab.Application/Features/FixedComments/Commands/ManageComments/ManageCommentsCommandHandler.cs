using MediatR;

namespace MasrLab.Application.Features.FixedComments.Commands.ManageComments;

public class ManageCommentsCommandHandler : IRequestHandler<ManageCommentsCommand, Unit>
{
    public Task<Unit> Handle(ManageCommentsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
