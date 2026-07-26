using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItems;

public class UpdatePriceListItemsCommandValidator : AbstractValidator<UpdatePriceListItemsCommand>
{
    public UpdatePriceListItemsCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0);
    }
}
