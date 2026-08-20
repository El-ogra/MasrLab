using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;

public class DeletePriceListCommandValidator : AbstractValidator<DeletePriceListCommand>
{
    public DeletePriceListCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0);
    }
}
