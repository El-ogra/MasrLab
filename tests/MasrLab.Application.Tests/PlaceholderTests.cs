using FluentValidation;
using MasrLab.Application.Common.Behaviors;
using MasrLab.Application.Common.Mappings;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Domain.Common.Enums;
using AutoMapper;
using MediatR;

namespace MasrLab.Application.Tests;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldPassThrough_WhenNoValidators()
    {
        var validators = new List<IValidator<RegisterPatientCommand>>();
        var behavior = new ValidationBehavior<RegisterPatientCommand, Unit>(validators);

        var command = new RegisterPatientCommand(
            "Ahmed", 30, 0, 0, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false, null);

        var called = false;
        RequestHandlerDelegate<Unit> next = ct =>
        {
            called = true;
            return Task.FromResult(Unit.Value);
        };
        var result = await behavior.Handle(command, next, CancellationToken.None);

        Assert.True(called);
        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var validator = new MockValidator(
            new FluentValidation.Results.ValidationFailure("Name", "Name is required"));

        var validators = new List<IValidator<RegisterPatientCommand>> { validator };
        var behavior = new ValidationBehavior<RegisterPatientCommand, Unit>(validators);

        var command = new RegisterPatientCommand(
            "", 30, 0, 0, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false, null);

        RequestHandlerDelegate<Unit> next = ct => Task.FromResult(Unit.Value);

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(command, next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationPasses()
    {
        var validator = new MockValidator();
        var validators = new List<IValidator<RegisterPatientCommand>> { validator };
        var behavior = new ValidationBehavior<RegisterPatientCommand, Unit>(validators);

        var command = new RegisterPatientCommand(
            "Ahmed", 30, 0, 0, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false, null);

        var nextCalled = false;
        RequestHandlerDelegate<Unit> next = ct =>
        {
            nextCalled = true;
            return Task.FromResult(Unit.Value);
        };

        await behavior.Handle(command, next, CancellationToken.None);
        Assert.True(nextCalled);
    }
}

public class RegisterPatientCommandValidatorTests
{
    private readonly RegisterPatientCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new RegisterPatientCommand(
            "", 30, 0, 0, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false, null);

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_LabId_Is_Empty()
    {
        var command = new RegisterPatientCommand(
            "Ahmed", 30, 0, 0, Gender.Male, null, null, null, null,
            "", 1, 1, AccountType.Cash, null, false, false, null);

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LabId");
    }

    [Fact]
    public void Should_Be_Valid_With_All_Required_Fields()
    {
        var command = new RegisterPatientCommand(
            "Ahmed", 30, 0, 0, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false, null);

        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}

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

public class AddTestCommandValidatorTests
{
    private readonly AddTestCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new AddTestCommand("", "Report", "Receipt", "Group", null, 100m, "1h", false, "mg/dL");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Zero()
    {
        var command = new AddTestCommand("CBC", "Report", "Receipt", "Group", null, 0m, "1h", false, "mg/dL");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_With_All_Required_Fields()
    {
        var command = new AddTestCommand("CBC", "Complete Blood Count", "CBC Receipt", "Hematology", null, 150m, "2h", false, "cells/uL");
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}

public class MappingProfileTests
{
    [Fact]
    public void AutoMapper_Profiles_ShouldBeRegistered()
    {
        var profile = new MappingProfile();
        Assert.NotNull(profile);
        Assert.False(string.IsNullOrEmpty(profile.ProfileName));
    }
}

internal class MockValidator : AbstractValidator<RegisterPatientCommand>
{
    public MockValidator(params FluentValidation.Results.ValidationFailure[] failures)
    {
        foreach (var failure in failures)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(failure.ErrorMessage);
        }
    }
}
