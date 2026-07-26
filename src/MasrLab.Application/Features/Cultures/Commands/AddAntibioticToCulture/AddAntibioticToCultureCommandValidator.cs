using FluentValidation;

namespace MasrLab.Application.Features.Cultures.Commands.AddAntibioticToCulture;

public class AddAntibioticToCultureCommandValidator : AbstractValidator<AddAntibioticToCultureCommand>
{
    public AddAntibioticToCultureCommandValidator()
    {
        RuleFor(x => x.CultureId).GreaterThan(0);
        RuleFor(x => x.AntibioticId).GreaterThan(0);
    }
}
