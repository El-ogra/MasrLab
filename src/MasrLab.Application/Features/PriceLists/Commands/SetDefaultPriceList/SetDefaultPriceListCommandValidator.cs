using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.SetDefaultPriceList;

public class SetDefaultPriceListCommandValidator : AbstractValidator<SetDefaultPriceListCommand>
{
    public SetDefaultPriceListCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0)
            .WithMessage("PriceListId must be greater than zero.");
    }
}
