using MasrLab.Application.Common.Models;

namespace MasrLab.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default);
    Task LogoutAsync(int userId, CancellationToken ct = default);
}
