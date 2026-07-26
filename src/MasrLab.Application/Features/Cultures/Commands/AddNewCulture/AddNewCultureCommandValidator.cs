using FluentValidation;

namespace MasrLab.Application.Features.Cultures.Commands.AddNewCulture;

public class AddNewCultureCommandValidator : AbstractValidator<AddNewCultureCommand>
{
    public AddNewCultureCommandValidator()
    {
        RuleFor(x => x.SampleType).NotEmpty();
        RuleFor(x => x.CultureCondition).NotEmpty();
        RuleFor(x => x.ColonyCount).GreaterThanOrEqualTo(0);
    }
}
