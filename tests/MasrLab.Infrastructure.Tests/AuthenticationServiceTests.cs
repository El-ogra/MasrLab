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
        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService.Setup(x => x.Now).Returns(DateTime.UtcNow);
        return new AuthenticationService(context, currentUserService.Object, dateTimeService.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsSuccess_WithValidCredentials()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword);
        var service = CreateService(context);

        var result = await service.LoginAsync("admin", ValidPassword);

        Assert.True(result.IsSuccess);
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

        Assert.False(result.IsSuccess);
        Assert.Null(result.UserId);
        Assert.NotEmpty(result.FailureReason);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WithNonExistentUsername()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword);
        var service = CreateService(context);

        var result = await service.LoginAsync("nonexistent", ValidPassword);

        Assert.False(result.IsSuccess);
        Assert.Null(result.UserId);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WithSoftDeletedUser()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword, isDeleted: true);
        var service = CreateService(context);

        var result = await service.LoginAsync("admin", ValidPassword);

        Assert.False(result.IsSuccess);
        Assert.Null(result.UserId);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUserRole_WhenUserIsNotAdmin()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("user", hashedPassword, isAdmin: false);
        var service = CreateService(context);

        var result = await service.LoginAsync("user", ValidPassword);

        Assert.True(result.IsSuccess);
        Assert.True(result.UserId > 0);
        Assert.Contains("User", result.Permissions);
        Assert.DoesNotContain("Admin", result.Permissions);
    }

    [Fact]
    public async Task LoginAsync_LocksOutAfterFiveFailedAttempts()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword);
        var service = CreateService(context);

        for (int i = 0; i < 5; i++)
            await service.LoginAsync("admin", "WrongPassword");

        var result = await service.LoginAsync("admin", "WrongPassword");

        Assert.False(result.IsSuccess);
        Assert.Contains("مقفل", result.FailureReason);
    }

    [Fact]
    public async Task LogoutAsync_ClearsCurrentUser()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ValidPassword);
        using var context = CreateContextWithUser("admin", hashedPassword);
        var currentUserService = new Mock<ICurrentUserService>();
        var dateTimeService = new Mock<IDateTimeService>();
        var service = new AuthenticationService(context, currentUserService.Object, dateTimeService.Object);

        await service.LogoutAsync(1);

        currentUserService.Verify(x => x.ClearCurrentUser(), Times.Once);
    }
}
