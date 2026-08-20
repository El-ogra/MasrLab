using MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;
using MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;
using MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;

namespace MasrLab.Application.Tests;

public class TestGroupValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void AddTestGroup_InvalidName_Fails(string name)
    {
        var v = new AddTestGroupCommandValidator();
        var result = v.Validate(new AddTestGroupCommand(name));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddTestGroup_ValidName_Passes()
    {
        var v = new AddTestGroupCommandValidator();
        var result = v.Validate(new AddTestGroupCommand("Checkup"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RenameTestGroup_ZeroId_Fails()
    {
        var v = new RenameTestGroupCommandValidator();
        var result = v.Validate(new RenameTestGroupCommand(0, "Name"));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void RenameTestGroup_EmptyName_Fails(string name)
    {
        var v = new RenameTestGroupCommandValidator();
        var result = v.Validate(new RenameTestGroupCommand(1, name));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeleteTestGroup_ZeroId_Fails()
    {
        var v = new DeleteTestGroupCommandValidator();
        var result = v.Validate(new DeleteTestGroupCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddTestToGroup_NegativePrice_Fails()
    {
        var v = new AddTestToGroupCommandValidator();
        var result = v.Validate(new AddTestToGroupCommand(1, 1, -1m));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddTestToGroup_Valid_Passes()
    {
        var v = new AddTestToGroupCommandValidator();
        var result = v.Validate(new AddTestToGroupCommand(1, 1, 50m));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateTestInGroup_NegativePrice_Fails()
    {
        var v = new UpdateTestInGroupCommandValidator();
        var result = v.Validate(new UpdateTestInGroupCommand(1, -5m));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTestInGroup_ZeroDisplayOrder_Fails()
    {
        var v = new UpdateTestInGroupCommandValidator();
        var result = v.Validate(new UpdateTestInGroupCommand(1, 10m, DisplayOrder: 0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTestInGroup_NullDisplayOrder_Passes()
    {
        var v = new UpdateTestInGroupCommandValidator();
        var result = v.Validate(new UpdateTestInGroupCommand(1, 10m, DisplayOrder: null));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RemoveTestFromGroup_ZeroId_Fails()
    {
        var v = new RemoveTestFromGroupCommandValidator();
        var result = v.Validate(new RemoveTestFromGroupCommand(0));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddTestGroup_NameExceeding200Chars_Fails()
    {
        var v = new AddTestGroupCommandValidator();
        var result = v.Validate(new AddTestGroupCommand(new string('A', 201)));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void RenameTestGroup_NameExceeding200Chars_Fails()
    {
        var v = new RenameTestGroupCommandValidator();
        var result = v.Validate(new RenameTestGroupCommand(1, new string('A', 201)));
        Assert.False(result.IsValid);
    }
}
