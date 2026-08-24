using FluentValidation;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.DeleteCultureAntibiotic;

public sealed class DeleteCultureAntibioticCommandValidator : AbstractValidator<DeleteCultureAntibioticCommand>
{
    public DeleteCultureAntibioticCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
