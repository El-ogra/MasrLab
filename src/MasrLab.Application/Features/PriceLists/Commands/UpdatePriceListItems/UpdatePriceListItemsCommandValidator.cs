using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItems;

public class UpdatePriceListItemsCommandValidator : AbstractValidator<UpdatePriceListItemsCommand>
{
    public UpdatePriceListItemsCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0);

        RuleForEach(x => x.Items)
            .Must(item => item.Price >= 0)
            .WithMessage("Price cannot be negative.");
    }
}
