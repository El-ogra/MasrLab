using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.SetPermissions;

public record SetPermissionsCommand : IRequest<Unit>;
