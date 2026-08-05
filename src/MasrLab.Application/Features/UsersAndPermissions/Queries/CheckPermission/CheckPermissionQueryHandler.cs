using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.UsersAndPermissions.Queries.CheckPermission;

public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IRepository<Permission> _permissionRepository;

    public CheckPermissionQueryHandler(IRepository<Permission> permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        var allPermissions = await _permissionRepository.GetAllAsync(cancellationToken);

        var permission = allPermissions.FirstOrDefault(p =>
            p.UserId == request.UserId &&
            p.ScreenId == (ScreenType)request.ScreenId &&
            p.OperationId == (PermissionOperation)request.OperationId);

        return permission?.Allowed ?? false;
    }
}
