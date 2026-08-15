using FluentValidation;

namespace MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;

public class AddTestsToVisitCommandValidator : AbstractValidator<AddTestsToVisitCommand>
{
    private static readonly HashSet<string> ValidSources = new(StringComparer.OrdinalIgnoreCase)
    {
        "Direct", "LegacyGroup", "SelectionGroup", "CommercialPackage"
    };

    public AddTestsToVisitCommandValidator()
    {
        RuleFor(x => x.PatientVisitId).GreaterThan(0);
        RuleFor(x => x.Source).NotEmpty().Must(s => ValidSources.Contains(s))
            .WithMessage("Source must be Direct, LegacyGroup, SelectionGroup, or CommercialPackage.");
        RuleFor(x => x.DirectTestIds)
            .NotEmpty()
            .When(x => x.Source == "Direct" || x.Source == "LegacyGroup");
        RuleFor(x => x.TestGroupId)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.Source == "SelectionGroup");
        RuleFor(x => x.CommercialPackageId)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.Source == "CommercialPackage");
    }
}
