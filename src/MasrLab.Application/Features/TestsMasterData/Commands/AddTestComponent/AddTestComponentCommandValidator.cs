using FluentValidation;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddTestComponent;

public class AddTestComponentCommandValidator : AbstractValidator<AddTestComponentCommand>
{
    public AddTestComponentCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();
        RuleFor(x => x.DisplayOrder).GreaterThan(0);
    }
}
