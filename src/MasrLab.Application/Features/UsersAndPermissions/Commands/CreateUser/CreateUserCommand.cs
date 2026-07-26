using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;

public record CreateUserCommand(
    string Username,
    string Password,
    bool IsAdmin
) : IRequest<Unit>;
