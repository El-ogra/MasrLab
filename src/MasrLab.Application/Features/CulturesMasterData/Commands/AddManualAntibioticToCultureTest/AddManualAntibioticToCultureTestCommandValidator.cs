using FluentValidation;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest;

public sealed class AddManualAntibioticToCultureTestCommandValidator : AbstractValidator<AddManualAntibioticToCultureTestCommand>
{
    public AddManualAntibioticToCultureTestCommandValidator()
    {
        RuleFor(x => x.CultureTestId).GreaterThan(0);
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ScientificName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SensitivityText).MaximumLength(200);
        RuleFor(x => x.CommercialNames)
            .Must(names => names is not null && names.Count <= 8)
            .WithMessage("A culture antibiotic can have at most 8 commercial names.");
        RuleForEach(x => x.CommercialNames)
            .ChildRules(name => name.RuleFor(n => n.Name).NotEmpty().MaximumLength(200));
    }
}
