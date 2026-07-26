using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Queries.CheckPermission;

public record CheckPermissionQuery : IRequest<bool>;
