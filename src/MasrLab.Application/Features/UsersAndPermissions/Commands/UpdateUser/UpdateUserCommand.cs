using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;

public record UpdateUserCommand(
    int Id,
    string Username,
    string Password,
    bool IsAdmin,
    bool IsActive
) : IRequest<Unit>;
