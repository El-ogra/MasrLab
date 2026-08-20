using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItem;

namespace MasrLab.Application.Tests;

public class UpdatePriceListItemCommandValidatorTests
{
    private readonly UpdatePriceListItemCommandValidator _validator = new();

    [Fact] public void ZeroId_Fails() => Assert.False(_validator.Validate(new UpdatePriceListItemCommand(0, 0m)).IsValid);
    [Fact] public void NegativePrice_Fails() => Assert.False(_validator.Validate(new UpdatePriceListItemCommand(1, -1m)).IsValid);
    [Fact] public void ZeroPrice_Passes() => Assert.True(_validator.Validate(new UpdatePriceListItemCommand(1, 0m)).IsValid);
}
