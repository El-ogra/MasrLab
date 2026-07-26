namespace MasrLab.Domain.Common;

public abstract class BaseEntity : IAuditableEntity, ISoftDeletable
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
}
