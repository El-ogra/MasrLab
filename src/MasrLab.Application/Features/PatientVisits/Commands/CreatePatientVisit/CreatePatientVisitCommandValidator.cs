using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;

public class CreatePatientVisitCommandValidator : AbstractValidator<CreatePatientVisitCommand>
{
    public CreatePatientVisitCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0)
            .WithMessage("PatientId must be greater than zero.");
    }
}
