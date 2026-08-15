using FluentValidation;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateTestComponent;

public class UpdateTestComponentCommandValidator : AbstractValidator<UpdateTestComponentCommand>
{
    public UpdateTestComponentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Unit).NotEmpty();
        RuleFor(x => x.DisplayOrder).GreaterThan(0);
    }
}
