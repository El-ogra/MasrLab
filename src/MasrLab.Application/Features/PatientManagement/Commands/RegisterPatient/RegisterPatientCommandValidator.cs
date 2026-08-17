using FluentValidation;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.LabId).NotEmpty();
        RuleFor(x => x.AgeYears).InclusiveBetween(0, 150);
        RuleFor(x => x.AgeMonths).InclusiveBetween(0, 11);
        RuleFor(x => x.AgeDays).InclusiveBetween(0, 30);
        RuleFor(x => x.AgeUnit).IsInEnum();
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}
