using FluentValidation;
using MasrLab.Application.Common.Behaviors;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;
using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Tests;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldPassThrough_WhenNoValidators()
    {
        var validators = new List<IValidator<RegisterPatientCommand>>();
        var behavior = new ValidationBehavior<RegisterPatientCommand, RegisterPatientResult>(validators);

        var command = new RegisterPatientCommand(
            "Ahmed", 30, 0, 0, AgeUnit.Years, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1);

        var called = false;
        RequestHandlerDelegate<RegisterPatientResult> next = ct =>
        {
            called = true;
            return Task.FromResult(new RegisterPatientResult(true, Array.Empty<DuplicatePatientDto>()));
        };
        var result = await behavior.Handle(command, next, CancellationToken.None);

        Assert.True(called);
        Assert.True(result.IsRegistered);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var validator = new MockValidator(
            new FluentValidation.Results.ValidationFailure("Name", "Name is required"));

        var validators = new List<IValidator<RegisterPatientCommand>> { validator };
        var behavior = new ValidationBehavior<RegisterPatientCommand, RegisterPatientResult>(validators);

        var command = new RegisterPatientCommand(
            "", 30, 0, 0, AgeUnit.Years, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1);

        RequestHandlerDelegate<RegisterPatientResult> next = ct =>
            Task.FromResult(new RegisterPatientResult(true, Array.Empty<DuplicatePatientDto>()));

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(command, next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationPasses()
    {
        var validator = new MockValidator();
        var validators = new List<IValidator<RegisterPatientCommand>> { validator };
        var behavior = new ValidationBehavior<RegisterPatientCommand, RegisterPatientResult>(validators);

        var command = new RegisterPatientCommand(
            "Ahmed", 30, 0, 0, AgeUnit.Years, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1);

        var nextCalled = false;
        RequestHandlerDelegate<RegisterPatientResult> next = ct =>
        {
            nextCalled = true;
            return Task.FromResult(new RegisterPatientResult(true, Array.Empty<DuplicatePatientDto>()));
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
