using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Unit>
{
    public Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
