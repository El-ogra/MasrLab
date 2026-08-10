namespace MasrLab.Application.Common.Models;

public record AuthResult
{
    public int? UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
    public bool IsSuccess => UserId.HasValue;
    public string FailureReason { get; init; } = string.Empty;

    public static AuthResult Success(int userId, string username, IReadOnlyList<string> permissions)
        => new() { UserId = userId, Username = username, Permissions = permissions };

    public static AuthResult Failure(string reason = "")
        => new() { FailureReason = reason };
}
