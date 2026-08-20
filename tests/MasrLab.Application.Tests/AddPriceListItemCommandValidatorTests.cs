using MasrLab.Application.Features.PriceLists.Commands.AddPriceListItem;

namespace MasrLab.Application.Tests;

public class AddPriceListItemCommandValidatorTests
{
    private readonly AddPriceListItemCommandValidator _validator = new();

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 1, -0.01)]
    public void Validate_WhenInvalid_Fails(int priceListId, int testId, decimal price)
        => Assert.False(_validator.Validate(new AddPriceListItemCommand(priceListId, testId, price)).IsValid);

    [Fact]
    public void Validate_WhenZeroPriceIsValid_Passes()
        => Assert.True(_validator.Validate(new AddPriceListItemCommand(1, 2, 0m)).IsValid);
}
