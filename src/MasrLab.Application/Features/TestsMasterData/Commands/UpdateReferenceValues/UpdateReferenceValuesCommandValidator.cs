using FluentValidation;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValues;

public class UpdateReferenceValuesCommandValidator : AbstractValidator<UpdateReferenceValuesCommand>
{
    public UpdateReferenceValuesCommandValidator()
    {
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.NormalRange).NotEmpty();
        RuleFor(x => x.AgeMin).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AgeMax).GreaterThan(0);
    }
}
