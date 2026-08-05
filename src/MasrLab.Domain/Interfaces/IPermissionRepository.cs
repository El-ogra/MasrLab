using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Domain.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByUserScreenOperationAsync(int userId, ScreenType screenId, PermissionOperation operationId, CancellationToken cancellationToken = default);
}
