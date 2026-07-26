using FluentValidation;

namespace MasrLab.Application.Features.Cultures.Commands.EnterCultureResult;

public class EnterCultureResultCommandValidator : AbstractValidator<EnterCultureResultCommand>
{
    public EnterCultureResultCommandValidator()
    {
        RuleFor(x => x.CultureId).GreaterThan(0);
        RuleFor(x => x.CultureCondition).NotEmpty();
        RuleFor(x => x.ColonyCount).GreaterThanOrEqualTo(0);
    }
}
