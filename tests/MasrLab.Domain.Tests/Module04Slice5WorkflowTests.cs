using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

// Slice 5 — OQ-M4-2 Finish→Verify→Print machine, OQ-M4-3 Export NO-OP marker.
public class Module04Slice5WorkflowTests
{
    private static VisitTest CreateVisitTest() => new(1, 1, 100m, false);

    [Fact]
    public void MarkFinished_SetsFlagWithAttribution()
    {
        var visitTest = CreateVisitTest();

        visitTest.MarkFinished(7);

        Assert.True(visitTest.IsFinished);
        Assert.Equal(7, visitTest.FinishedByUserId);
        Assert.NotNull(visitTest.FinishedAt);
        Assert.False(visitTest.IsVerified);
        Assert.False(visitTest.IsPrinted);
    }

    [Fact]
    public void MarkVerified_WithoutFinish_ShouldThrowBusinessRuleViolation()
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(() => CreateVisitTest().MarkVerified(7));
        Assert.Equal("A test must be finished before it can be verified.", ex.Message);
    }

    [Fact]
    public void MarkPrinted_WithoutVerify_ShouldThrowBusinessRuleViolation_OQ_M4_2()
    {
        var visitTest = CreateVisitTest();
        visitTest.MarkFinished(7);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visitTest.MarkPrinted(7));

        Assert.Equal("A test must be verified before it can be printed.", ex.Message);
        Assert.False(visitTest.IsPrinted); // print physically blocked.
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MarkMethods_ShouldRejectInvalidUserIds(int userId)
    {
        var visitTest = CreateVisitTest();
        Assert.Throws<BusinessRuleViolationException>(() => visitTest.MarkFinished(userId));
        visitTest.MarkFinished(7);
        Assert.Throws<BusinessRuleViolationException>(() => visitTest.MarkVerified(userId));
        visitTest.MarkVerified(7);
        Assert.Throws<BusinessRuleViolationException>(() => visitTest.MarkPrinted(userId));
    }

    [Fact]
    public void FullWorkflow_FinishThenVerifyThenPrint_TransitionsMonotonically()
    {
        var visitTest = CreateVisitTest();

        visitTest.MarkFinished(7);
        visitTest.MarkVerified(9);
        visitTest.MarkPrinted(11);

        Assert.True(visitTest.IsFinished && visitTest.IsVerified && visitTest.IsPrinted);
        Assert.Equal((7, 9, 11), (visitTest.FinishedByUserId, visitTest.VerifiedByUserId, visitTest.PrintedByUserId));

        // Flag monotonicity: re-invocations are no-ops and keep original attribution.
        visitTest.MarkFinished(99);
        visitTest.MarkVerified(99);
        visitTest.MarkPrinted(99);
        Assert.Equal((7, 9, 11), (visitTest.FinishedByUserId, visitTest.VerifiedByUserId, visitTest.PrintedByUserId));
    }

    // OQ-M4-3: setting the Export flag emits no events and touches nothing else.
    [Fact]
    public void SetExportMark_IsAPureStoredNoOp()
    {
        var visitTest = CreateVisitTest();

        visitTest.SetExportMark(true);

        Assert.True(visitTest.IsExportMarked);
        Assert.Empty(visitTest.DomainEvents);
        Assert.False(visitTest.IsFinished);
        Assert.False(visitTest.IsVerified);
        Assert.False(visitTest.IsPrinted);

        visitTest.SetExportMark(false);
        Assert.False(visitTest.IsExportMarked);
    }

    [Fact]
    public void AccountType_WorklistCategories_AreAvailableForOQ_M4_10()
    {
        Assert.True(Enum.IsDefined(typeof(AccountType), AccountType.Individual));
        Assert.True(Enum.IsDefined(typeof(AccountType), AccountType.LabToLab));
        Assert.True(Enum.IsDefined(typeof(AccountType), AccountType.VIP));
        Assert.True(Enum.IsDefined(typeof(AccountType), AccountType.Free));
        Assert.Equal(0, (int)AccountType.Cash);   // drawer-accounting values stay stable.
        Assert.Equal(1, (int)AccountType.Insurance);
        Assert.Equal(2, (int)AccountType.Contract);
    }
}
