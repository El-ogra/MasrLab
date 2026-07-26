using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;

public record CreateUserCommand : IRequest<Unit>;
