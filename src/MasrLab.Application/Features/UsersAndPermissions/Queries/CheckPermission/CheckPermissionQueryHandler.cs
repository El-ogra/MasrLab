using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.UsersAndPermissions.Queries.CheckPermission;

public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    private readonly IPermissionRepository _permissionRepository;

    public CheckPermissionQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByUserScreenOperationAsync(
            request.UserId,
            (ScreenType)request.ScreenId,
            (PermissionOperation)request.OperationId,
            cancellationToken);

        return permission?.Allowed ?? false;
    }
}
