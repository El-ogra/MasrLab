using System.Collections.Concurrent;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Models;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 5;

    private readonly MasrLabDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    private static readonly ConcurrentDictionary<string, (int Count, DateTime? LockedUntil)> _failedAttempts = new();

    public AuthenticationService(
        MasrLabDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (IsLockedOut(username))
        {
            var remaining = GetLockoutRemaining(username);
            return AuthResult.Failure($"الحساب مقفل مؤقتاً. حاول بعد {remaining.Minutes + 1} دقائق.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            RecordFailedAttempt(username);
            return AuthResult.Failure("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        ResetFailedAttempts(username);

        var role = user.IsAdmin ? "Admin" : "User";
        _currentUserService.SetCurrentUser(user.Id, user.Username, role);

        return AuthResult.Success(user.Id, user.Username, new List<string> { role });
    }

    public Task LogoutAsync(int userId, CancellationToken ct = default)
    {
        _currentUserService.ClearCurrentUser();
        return Task.CompletedTask;
    }

    private bool IsLockedOut(string username)
    {
        if (!_failedAttempts.TryGetValue(username, out var entry))
            return false;

        if (entry.LockedUntil is null)
            return false;

        if (_dateTimeService.Now >= entry.LockedUntil.Value)
        {
            _failedAttempts.TryRemove(username, out _);
            return false;
        }

        return true;
    }

    private TimeSpan GetLockoutRemaining(string username)
    {
        if (_failedAttempts.TryGetValue(username, out var entry) && entry.LockedUntil.HasValue)
            return entry.LockedUntil.Value - _dateTimeService.Now;
        return TimeSpan.Zero;
    }

    private void RecordFailedAttempt(string username)
    {
        _failedAttempts.AddOrUpdate(username,
            (1, null),
            (_, existing) =>
            {
                var newCount = existing.Count + 1;
                if (newCount >= MaxFailedAttempts)
                    return (newCount, _dateTimeService.Now.AddMinutes(LockoutMinutes));
                return (newCount, null);
            });
    }

    private void ResetFailedAttempts(string username)
    {
        _failedAttempts.TryRemove(username, out _);
    }
}
