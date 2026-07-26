using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.SetPermissions;

public record SetPermissionsCommand(
    int UserId,
    int ScreenId,
    int OperationId,
    bool Allowed
) : IRequest<Unit>;
