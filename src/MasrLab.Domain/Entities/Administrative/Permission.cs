using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Administrative;

public class Permission : BaseEntity
{
    public int UserId { get; set; }
    public int ScreenId { get; set; }
    public int OperationId { get; set; }
    public bool Allowed { get; set; }
}
