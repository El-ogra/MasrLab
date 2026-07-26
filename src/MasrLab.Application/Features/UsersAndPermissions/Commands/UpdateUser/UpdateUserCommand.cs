using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<Unit>;
