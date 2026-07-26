using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Queries.CheckPermission;

public record CheckPermissionQuery(int UserId, int ScreenId, int OperationId) : IRequest<bool>;
