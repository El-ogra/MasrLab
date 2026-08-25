using FluentValidation;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;

public class UpdatePatientDataCommandValidator : AbstractValidator<UpdatePatientDataCommand>
{
    public UpdatePatientDataCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
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
    }
}
