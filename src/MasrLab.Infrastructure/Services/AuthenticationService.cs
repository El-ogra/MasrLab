using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Models;

namespace MasrLab.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    public Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(int userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
