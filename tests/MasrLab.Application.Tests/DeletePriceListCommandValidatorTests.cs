using MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;

namespace MasrLab.Application.Tests;

public class DeletePriceListCommandValidatorTests
{
    private readonly DeletePriceListCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenPriceListIdIsZero_ShouldFail()
    {
        var command = new DeletePriceListCommand(0);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPriceListIdIsNegative_ShouldFail()
    {
        var command = new DeletePriceListCommand(-1);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_ShouldPass()
    {
        var command = new DeletePriceListCommand(5);
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
