using MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;

namespace MasrLab.Application.Tests;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var command = new CreateUserCommand("", "password123", false);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        var command = new CreateUserCommand("admin", "123", false);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_With_Valid_Data()
    {
        var command = new CreateUserCommand("admin", "password123", true);
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
