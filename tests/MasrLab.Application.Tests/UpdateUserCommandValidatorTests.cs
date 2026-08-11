using MasrLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;

namespace MasrLab.Application.Tests;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator = new();

    [Fact]
    public void Should_Be_Valid_With_Valid_Password()
    {
        var command = new UpdateUserCommand(1, "admin", "password123", true, true);
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_With_Empty_Password()
    {
        var command = new UpdateUserCommand(1, "admin", "", true, true);
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_With_Whitespace_Password()
    {
        var command = new UpdateUserCommand(1, "admin", "   ", true, true);
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        var command = new UpdateUserCommand(1, "admin", "123", true, true);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Zero()
    {
        var command = new UpdateUserCommand(0, "admin", "password123", true, true);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var command = new UpdateUserCommand(1, "", "password123", true, true);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Too_Short()
    {
        var command = new UpdateUserCommand(1, "ab", "password123", true, true);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }
}
