using FluentValidation;

namespace MasrLab.Application.Features.PatientVisits.Commands.AddTestToVisit;

public class AddTestToVisitCommandValidator : AbstractValidator<AddTestToVisitCommand>
{
    public AddTestToVisitCommandValidator()
    {
        RuleFor(x => x.PatientVisitId)
            .GreaterThan(0)
            .WithMessage("PatientVisitId must be greater than zero.");

        RuleFor(x => x.TestIds)
            .NotEmpty()
            .WithMessage("At least one test must be provided.");

        RuleFor(x => x.TestIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate test IDs are not allowed.");

        RuleFor(x => x.PriceListId)
            .GreaterThan(0)
            .When(x => x.PriceListId.HasValue)
            .WithMessage("PriceListId must be greater than zero when specified.");
    }
}
