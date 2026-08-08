using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItems;

namespace MasrLab.Application.Tests;

public class UpdatePriceListItemsCommandValidatorTests
{
    private readonly UpdatePriceListItemsCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenItemPriceIsNegative_ShouldFail()
    {
        var command = new UpdatePriceListItemsCommand(
            1,
            new List<PriceListItemData> { new(10, -5m) });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenItemPriceIsZero_ShouldPass()
    {
        var command = new UpdatePriceListItemsCommand(
            1,
            new List<PriceListItemData> { new(10, 0m) });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenItemPriceIsPositive_ShouldPass()
    {
        var command = new UpdatePriceListItemsCommand(
            1,
            new List<PriceListItemData> { new(10, 150m) });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
