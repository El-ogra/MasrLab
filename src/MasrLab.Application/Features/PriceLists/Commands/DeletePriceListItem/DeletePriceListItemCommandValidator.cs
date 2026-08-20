using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceListItem;

public class DeletePriceListItemCommandValidator : AbstractValidator<DeletePriceListItemCommand>
{
    public DeletePriceListItemCommandValidator()
    {
        RuleFor(x => x.PriceListItemId).GreaterThan(0);
    }
}
