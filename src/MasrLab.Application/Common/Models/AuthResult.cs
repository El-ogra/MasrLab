namespace MasrLab.Application.Common.Models;

public record AuthResult(int UserId, string Username, IReadOnlyList<string> Permissions);
