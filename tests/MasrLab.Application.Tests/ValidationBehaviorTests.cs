using FluentValidation;
using MasrLab.Application.Common.Behaviors;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Domain.Common.Enums;
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
            "Ahmed", 30, 0, 0, AgeUnit.Years, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false,
            false, false, false, false, false, false, false, false, null);

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
            "", 30, 0, 0, AgeUnit.Years, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false,
            false, false, false, false, false, false, false, false, null);

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
            "Ahmed", 30, 0, 0, AgeUnit.Years, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false,
            false, false, false, false, false, false, false, false, null);

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
