using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Administrative;

public class Permission : BaseEntity
{
    public int UserId { get; set; }
    public ScreenType ScreenId { get; set; }
    public PermissionOperation OperationId { get; set; }
    public bool Allowed { get; set; }
}
