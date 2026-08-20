using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;

namespace MasrLab.Application.Tests;

public class UpdatePriceListNameCommandValidatorTests
{
    private readonly UpdatePriceListNameCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenPriceListIdIsZero_ShouldFail()
    {
        var command = new UpdatePriceListNameCommand(0, "Valid Name");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldFail()
    {
        var command = new UpdatePriceListNameCommand(1, "");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_ShouldPass()
    {
        var command = new UpdatePriceListNameCommand(1, "New Name");
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
