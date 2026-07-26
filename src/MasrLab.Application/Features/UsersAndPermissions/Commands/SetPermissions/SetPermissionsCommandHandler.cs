using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.SetPermissions;

public class SetPermissionsCommandHandler : IRequestHandler<SetPermissionsCommand, Unit>
{
    public Task<Unit> Handle(SetPermissionsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
