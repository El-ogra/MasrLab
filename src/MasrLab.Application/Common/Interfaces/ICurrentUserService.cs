namespace MasrLab.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? Role { get; }
    void SetCurrentUser(int userId, string username, string role) { }
    void ClearCurrentUser() { }
}
