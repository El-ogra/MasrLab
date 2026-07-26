using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Administrative;

public class AuditLog : BaseEntity
{
    public int UserId { get; set; }
    public AuditActionType ActionType { get; set; }
    public AuditEntityType EntityType { get; set; }
    public int EntityId { get; set; }
    public DateTime ActionTime { get; set; }
    public int PrintCount { get; set; }
}
