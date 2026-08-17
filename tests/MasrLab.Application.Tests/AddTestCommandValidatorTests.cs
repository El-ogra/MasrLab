using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

public class AddTestCommandValidatorTests
{
    private readonly AddTestCommandValidator _validator = new();

    private static AddTestCommand CreateCommand(string name = "CBC", decimal price = 100m) =>
        new(name, "Report", "Receipt", "Group", null, price, "1h", false, "mg/dL",
            null, null, null, null, null, null, false, false, false, false, 1, 0,
            ReferenceType.General, null, null, null, null, null, false, null, null, null);

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = CreateCommand(name: "");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Zero()
    {
        var command = CreateCommand(price: 0m);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_With_All_Required_Fields()
    {
        var command = CreateCommand();
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_When_CostPrice_Is_Null()
    {
        var command = CreateCommand();
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_When_CostPrice_Is_Zero()
    {
        var command = CreateCommand() with { CostPrice = 0m };
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_CostPrice_Is_Negative()
    {
        var command = CreateCommand() with { CostPrice = -1m };
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }
}
