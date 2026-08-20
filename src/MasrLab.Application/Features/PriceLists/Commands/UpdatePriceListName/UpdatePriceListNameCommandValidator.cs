using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;

public class UpdatePriceListNameCommandValidator : AbstractValidator<UpdatePriceListNameCommand>
{
    public UpdatePriceListNameCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
