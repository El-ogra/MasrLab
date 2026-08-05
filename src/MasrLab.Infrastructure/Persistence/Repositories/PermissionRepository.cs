using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<Permission?> GetByUserScreenOperationAsync(int userId, ScreenType screenId, PermissionOperation operationId, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.ScreenId == screenId &&
                p.OperationId == operationId, cancellationToken);
    }
}
