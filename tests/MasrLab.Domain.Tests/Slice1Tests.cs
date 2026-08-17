using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class Slice1Tests
{
    #region TestResult.Comment

    [Fact]
    public void TestResult_SetComment_WithValidComment_ShouldStore()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        result.SetComment("Normal range");

        Assert.Equal("Normal range", result.Comment);
    }

    [Fact]
    public void TestResult_SetComment_WithNull_ShouldClear()
    {
        var result = TestResult.Enter(5, "12.5", 7);
        result.SetComment("Some comment");

        result.SetComment(null);

        Assert.Null(result.Comment);
    }

    [Fact]
    public void TestResult_SetComment_WhenExceeds1000_ShouldThrow()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => result.SetComment(new string('a', 1001)));

        Assert.Equal("Comment cannot exceed 1000 characters.", ex.Message);
    }

    [Fact]
    public void TestResult_SetComment_AtExactly1000_ShouldAccept()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        result.SetComment(new string('a', 1000));

        Assert.Equal(1000, result.Comment!.Length);
    }

    #endregion

    #region TestResult.ReprintRequired

    [Fact]
    public void TestResult_ReprintRequired_Default_ShouldBeFalse()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        Assert.False(result.ReprintRequired);
    }

    [Fact]
    public void TestResult_MarkReprintRequired_ShouldSetTrue()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        result.MarkReprintRequired();

        Assert.True(result.ReprintRequired);
    }

    #endregion

    #region TestResult.MarkPrinted

    [Fact]
    public void TestResult_MarkPrinted_ShouldIncrementPrintCount()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        result.MarkPrinted(10);

        Assert.Equal(1, result.PrintCount);
        Assert.Equal(10, result.PrintedByUserId);
        Assert.NotNull(result.PrintedAt);
    }

    [Fact]
    public void TestResult_MarkPrinted_ShouldClearReprintRequired()
    {
        var result = TestResult.Enter(5, "12.5", 7);
        result.MarkReprintRequired();

        result.MarkPrinted(10);

        Assert.False(result.ReprintRequired);
    }

    [Fact]
    public void TestResult_MarkPrinted_CalledTwice_ShouldIncrementTwice()
    {
        var result = TestResult.Enter(5, "12.5", 7);

        result.MarkPrinted(10);
        result.MarkPrinted(10);

        Assert.Equal(2, result.PrintCount);
    }

    #endregion

    #region TestResult.Edit expanded event

    [Fact]
    public void TestResult_Edit_ShouldRaiseEventWithOldAndNewCommentAndChangeType()
    {
        var result = TestResult.Enter(5, "12.5", 7);
        result.SetComment("old comment");

        result.Edit("13.0", null, 9);

        var evt = Assert.Single(result.DomainEvents.OfType<TestResultEdited>());
        Assert.Equal("12.5", evt.OldValue);
        Assert.Equal("13.0", evt.NewValue);
        Assert.Equal("old comment", evt.OldComment);
        Assert.Null(evt.NewComment);
        Assert.Equal(ResultEditChangeType.ValueAndComment, evt.ChangeType);
        Assert.Equal(9, evt.EditedBy);
    }

    #endregion

    #region ResultEditChangeType enum

    [Fact]
    public void ResultEditChangeType_HasExpectedValues()
    {
        Assert.Equal(0, (int)ResultEditChangeType.ValueOnly);
        Assert.Equal(1, (int)ResultEditChangeType.CommentOnly);
        Assert.Equal(2, (int)ResultEditChangeType.ValueAndComment);
    }

    #endregion

    #region TestComponentChoice uniqueness

    [Fact]
    public void TestComponentChoice_ShouldHaveRequiredProperties()
    {
        var choice = new TestComponentChoice
        {
            TestComponentId = 1,
            Value = "Normal",
            DisplayOrder = 1,
            IsActive = true
        };

        Assert.Equal(1, choice.TestComponentId);
        Assert.Equal("Normal", choice.Value);
        Assert.Equal(1, choice.DisplayOrder);
        Assert.True(choice.IsActive);
    }

    #endregion

    #region PermissionOperation.EditPrinted

    [Fact]
    public void PermissionOperation_EditPrinted_ShouldBe7()
    {
        Assert.Equal(7, (int)PermissionOperation.EditPrinted);
    }

    #endregion

    #region CulturePrintReceipt

    [Fact]
    public void CulturePrintReceipt_ShouldHaveRequiredProperties()
    {
        var receipt = new CulturePrintReceipt
        {
            VisitTestResultItemId = 10,
            PrintedByUserId = 5,
            PrintedAt = DateTime.UtcNow,
            PrintCount = 1
        };

        Assert.Equal(10, receipt.VisitTestResultItemId);
        Assert.Equal(5, receipt.PrintedByUserId);
        Assert.Equal(1, receipt.PrintCount);
    }

    #endregion
}
