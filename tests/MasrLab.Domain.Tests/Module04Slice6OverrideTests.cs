using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

// Slice 6 — OQ-M4-4 manual H/L override with attribution.
public class Module04Slice6OverrideTests
{
    private static TestResult CreateEnteredResult()
    {
        var result = TestResult.Enter(10, "7.2", 5);
        typeof(TestResult).GetProperty(nameof(BaseEntity.Id))!.SetValue(result, 33);
        return result;
    }

    [Fact]
    public void OverrideStatus_WhenForced_SetsFlagAndAttribution()
    {
        var result = CreateEnteredResult();
        var statusBeforeOverride = result.Status;

        result.OverrideStatus(ResultStatus.Low, 9);

        Assert.NotEqual(statusBeforeOverride, result.Status);
        Assert.Equal(ResultStatus.Low, result.Status);
        Assert.True(result.IsStatusOverridden);
        Assert.Equal(9, result.EditedByUserId);
        Assert.NotNull(result.EditedAt);
    }

    [Fact]
    public void OverrideStatus_WhenCleared_RemovesFlagForEngineRecompute()
    {
        var result = CreateEnteredResult();
        result.OverrideStatus(ResultStatus.Low, 9);

        result.OverrideStatus(null, 9);

        Assert.False(result.IsStatusOverridden);
        Assert.Equal(9, result.EditedByUserId);
    }

    [Fact]
    public void OverrideStatus_ClearWithoutExistingOverride_ShouldThrowBusinessRuleViolation()
    {
        var result = CreateEnteredResult();

        Assert.Throws<BusinessRuleViolationException>(() => result.OverrideStatus(null, 9));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void OverrideStatus_WhenUserIdInvalid_ShouldThrowBusinessRuleViolation(int userId)
    {
        Assert.Throws<BusinessRuleViolationException>(
            () => CreateEnteredResult().OverrideStatus(ResultStatus.High, userId));
    }
}
