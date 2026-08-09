using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;

namespace MasrLab.Application.Tests;

public class AddTestCommandValidatorTests
{
    private readonly AddTestCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new AddTestCommand("", "Report", "Receipt", "Group", null, 100m, "1h", false, "mg/dL");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Zero()
    {
        var command = new AddTestCommand("CBC", "Report", "Receipt", "Group", null, 0m, "1h", false, "mg/dL");
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Be_Valid_With_All_Required_Fields()
    {
        var command = new AddTestCommand("CBC", "Complete Blood Count", "CBC Receipt", "Hematology", null, 150m, "2h", false, "cells/uL");
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
