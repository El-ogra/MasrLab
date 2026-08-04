using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record AuditLogDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public AuditActionType ActionType { get; init; }
    public AuditEntityType EntityType { get; init; }
    public int EntityId { get; init; }
    public DateTime ActionTime { get; init; }
    public int PrintCount { get; init; }
}
