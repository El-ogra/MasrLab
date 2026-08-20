using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.AddPriceListItem;

public class AddPriceListItemCommandValidator : AbstractValidator<AddPriceListItemCommand>
{
    public AddPriceListItemCommandValidator()
    {
        RuleFor(x => x.PriceListId).GreaterThan(0);
        RuleFor(x => x.TestId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
