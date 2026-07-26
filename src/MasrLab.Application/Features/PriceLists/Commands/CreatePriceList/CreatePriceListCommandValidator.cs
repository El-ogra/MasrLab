using FluentValidation;

namespace MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;

public class CreatePriceListCommandValidator : AbstractValidator<CreatePriceListCommand>
{
    public CreatePriceListCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
