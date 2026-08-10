using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MasrLab.Infrastructure.Tests;

public class AuthenticationServiceTests
{
    private const string ValidPassword = "TestPassword123";

    private MasrLabDbContext CreateContextWithUser(string username, string password, bool isAdmin = true, bool isDeleted = false)
    {
        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new MasrLabDbContext(options);

        context.Users.Add(new User
        {
            Username = username,
            Password = password,
            IsAdmin = isAdmin,
            IsActive = true,
            IsDeleted = isDeleted
        });
        context.SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
        return context;
    }

    private AuthenticationService CreateService(MasrLabDbContext context)
    {
        var currentUserService = new Mock<ICurrentUserService>();
        return new AuthenticationService(context, currentUserService.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsSuccess_WithValidCredentials()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword);
        var service = CreateService(context);

        var result = await service.LoginAsync("admin", ValidPassword);

        Assert.True(result.UserId > 0);
        Assert.Equal("admin", result.Username);
        Assert.Contains("Admin", result.Permissions);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WithWrongPassword()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword);
        var service = CreateService(context);

        var result = await service.LoginAsync("admin", "WrongPassword123");

        Assert.Equal(0, result.UserId);
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WithNonExistentUsername()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword);
        var service = CreateService(context);

        var result = await service.LoginAsync("nonexistent", ValidPassword);

        Assert.Equal(0, result.UserId);
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WithSoftDeletedUser()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword, isDeleted: true);
        var service = CreateService(context);

        var result = await service.LoginAsync("admin", ValidPassword);

        Assert.Equal(0, result.UserId);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUserRole_WhenUserIsNotAdmin()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("user", hashedPassword, isAdmin: false);
        var service = CreateService(context);

        var result = await service.LoginAsync("user", ValidPassword);

        Assert.True(result.UserId > 0);
        Assert.Contains("User", result.Permissions);
        Assert.DoesNotContain("Admin", result.Permissions);
    }

    [Fact]
    public async Task LogoutAsync_ClearsCurrentUser()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword);
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new AuthenticationService(context, currentUserService.Object);

        await service.LogoutAsync(1);

        currentUserService.Verify(x => x.ClearCurrentUser(), Times.Once);
    }
}
