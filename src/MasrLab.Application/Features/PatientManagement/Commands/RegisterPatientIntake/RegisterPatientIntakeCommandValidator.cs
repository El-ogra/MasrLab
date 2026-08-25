using FluentValidation;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatientIntake;

public sealed class RegisterPatientIntakeCommandValidator : AbstractValidator<RegisterPatientIntakeCommand>
{
    public RegisterPatientIntakeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.LabId).NotEmpty();
        RuleFor(x => x.AgeYears).InclusiveBetween(0, 150);
        RuleFor(x => x.AgeMonths).InclusiveBetween(0, 11);
        RuleFor(x => x.AgeDays).InclusiveBetween(0, 30);
        RuleFor(x => x.AgeUnit).IsInEnum();
        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x)
            .Must(x => x.AgeYears > 0 || x.AgeMonths > 0 || x.AgeDays > 0)
            .WithMessage("Age must be greater than zero.");
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Source)
            .Must(source => source is "Direct" or "LegacyGroup" or "SelectionGroup" or "CommercialPackage")
            .WithMessage("Unknown source.");
        RuleFor(x => x.TestGroupId)
            .NotNull()
            .When(x => x.Source == "SelectionGroup")
            .WithMessage("TestGroupId is required for SelectionGroup source.");
        RuleFor(x => x.CommercialPackageId)
            .NotNull()
            .When(x => x.Source == "CommercialPackage")
            .WithMessage("CommercialPackageId is required for CommercialPackage source.");
    }
}
