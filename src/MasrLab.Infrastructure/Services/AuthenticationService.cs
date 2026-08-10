using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Models;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly MasrLabDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuthenticationService(MasrLabDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return new AuthResult(0, string.Empty, Array.Empty<string>());
        }

        var role = user.IsAdmin ? "Admin" : "User";
        _currentUserService.SetCurrentUser(user.Id, user.Username, role);

        return new AuthResult(user.Id, user.Username, new List<string> { role });
    }

    public Task LogoutAsync(int userId, CancellationToken ct = default)
    {
        _currentUserService.ClearCurrentUser();
        return Task.CompletedTask;
    }
}
