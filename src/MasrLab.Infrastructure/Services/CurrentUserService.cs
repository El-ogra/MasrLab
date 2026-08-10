using MasrLab.Application.Common.Interfaces;

namespace MasrLab.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly object _lock = new();
    private int? _userId;
    private string? _username;
    private string? _role;

    public int? UserId
    {
        get { lock (_lock) return _userId; }
    }

    public string? Username
    {
        get { lock (_lock) return _username; }
    }

    public string? Role
    {
        get { lock (_lock) return _role; }
    }

    public void SetCurrentUser(int userId, string username, string role)
    {
        lock (_lock)
        {
            _userId = userId;
            _username = username;
            _role = role;
        }
    }

    public void ClearCurrentUser()
    {
        lock (_lock)
        {
            _userId = null;
            _username = null;
            _role = null;
        }
    }
}
