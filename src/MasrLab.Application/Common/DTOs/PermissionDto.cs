using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record PermissionDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public ScreenType ScreenId { get; init; }
    public PermissionOperation OperationId { get; init; }
    public bool Allowed { get; init; }
}
