using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;

public class ManageTestGroupsCommandHandler : IRequestHandler<ManageTestGroupsCommand, Unit>
{
    public Task<Unit> Handle(ManageTestGroupsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
