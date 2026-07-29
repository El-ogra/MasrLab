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
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}
