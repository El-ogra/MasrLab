using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItem;

public class UpdatePriceListItemCommandValidator : AbstractValidator<UpdatePriceListItemCommand>
{
    public UpdatePriceListItemCommandValidator()
    {
        RuleFor(x => x.PriceListItemId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
