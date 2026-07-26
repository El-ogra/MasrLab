namespace MasrLab.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    int CreatedByUserId { get; set; }
    DateTime? UpdatedAt { get; set; }
    int? UpdatedByUserId { get; set; }
}
