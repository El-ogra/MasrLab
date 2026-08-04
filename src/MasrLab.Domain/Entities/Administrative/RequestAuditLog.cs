using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Administrative;

public class RequestAuditLog : BaseEntity
{
    public string RequestName { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public DateTime ActionTimeUtc { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
