using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;
using Moq;

namespace MasrLab.Application.Tests;

public class ResultValidationServiceTests
{
    private readonly Mock<IReferenceValueRepository> _referenceValues;
    private readonly Mock<IVisitTestResultItemRepository> _visitTestResultItems;
    private readonly Mock<IReferenceValueMatcher> _matcher;

    public ResultValidationServiceTests()
    {
        _referenceValues = new Mock<IReferenceValueRepository>();
        _visitTestResultItems = new Mock<IVisitTestResultItemRepository>();
        _matcher = new Mock<IReferenceValueMatcher>();
    }

    private ResultValidationService CreateService()
        => new(_referenceValues.Object, _visitTestResultItems.Object, _matcher.Object);

    private void SetupResultItem(int id, int sourceTestComponentId)
    {
        _visitTestResultItems
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VisitTestResultItem
            {
                Id = id,
                VisitTestId = 10,
                SourceTestComponentId = sourceTestComponentId,
                ComponentName = "Test",
                ComponentUnit = "Unit",
                DisplayOrder = 1,
                ResultEntryKind = ResultEntryKind.Ordinary
            });
    }

    [Fact]
    public async Task ValidateResult_NonNumericValue_ReturnsNormal()
    {
        var service = CreateService();

        var result = await service.ValidateResultAsync(
            1, "abc", "male", new Age(30, 0, 0), false);

        Assert.Equal(ResultStatus.Normal, result.Status);
        Assert.Equal(ReferenceMatchKind.NoRangeConfigured, result.MatchKind);
    }

    [Fact]
    public async Task ValidateResult_CultureDetail_ReturnsNormal()
    {
        _visitTestResultItems
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VisitTestResultItem
            {
                Id = 1,
                VisitTestId = 10,
                SourceTestComponentId = 1,
                ComponentName = "Culture",
                ComponentUnit = "",
                DisplayOrder = 1,
                ResultEntryKind = ResultEntryKind.CultureDetail
            });

        var service = CreateService();

        var result = await service.ValidateResultAsync(
            1, "5.0", "male", new Age(30, 0, 0), false);

        Assert.Equal(ResultStatus.Normal, result.Status);
        Assert.Equal(ReferenceMatchKind.NoRangeConfigured, result.MatchKind);
    }

    [Fact]
    public async Task ValidateResult_NoRangeConfigured_ReturnsNormal()
    {
        SetupResultItem(1, 1);
        _referenceValues
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue>());

        _matcher
            .Setup(m => m.Match(It.IsAny<IReadOnlyList<ReferenceValue>>(), 1, "male", It.IsAny<Age>(), false))
            .Returns(ReferenceMatchResult.NoRangeConfigured());

        var service = CreateService();

        var result = await service.ValidateResultAsync(
            1, "5.0", "male", new Age(30, 0, 0), false);

        Assert.Equal(ResultStatus.Normal, result.Status);
        Assert.Equal(ReferenceMatchKind.NoRangeConfigured, result.MatchKind);
    }

    [Fact]
    public async Task ValidateResult_NoRangeForDemographics_ReturnsNormalWithWarningKind()
    {
        SetupResultItem(1, 1);
        _referenceValues
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue>
            {
                new() { Id = 1, TestId = 1, TestComponentId = 1, Gender = ReferenceValueGender.Female,
                    AgeMin = 0, AgeMax = 0, AgeUnit = AgeUnit.Years, NormalRange = "4-10" }
            });

        _matcher
            .Setup(m => m.Match(It.IsAny<IReadOnlyList<ReferenceValue>>(), 1, "male", It.IsAny<Age>(), false))
            .Returns(ReferenceMatchResult.NoRangeForDemographics());

        var service = CreateService();

        var result = await service.ValidateResultAsync(
            1, "5.0", "male", new Age(30, 0, 0), false);

        Assert.Equal(ResultStatus.Normal, result.Status);
        Assert.Equal(ReferenceMatchKind.NoRangeForDemographics, result.MatchKind);
    }

    [Fact]
    public async Task ValidateResult_InRange_ReturnsNormal()
    {
        SetupResultItem(1, 1);
        _referenceValues
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue>
            {
                new() { Id = 1, TestId = 1, TestComponentId = 1, Gender = ReferenceValueGender.Both,
                    AgeMin = 0, AgeMax = 0, AgeUnit = AgeUnit.Years, NormalRange = "4-10" }
            });

        _matcher
            .Setup(m => m.Match(It.IsAny<IReadOnlyList<ReferenceValue>>(), 1, "male", It.IsAny<Age>(), false))
            .Returns(ReferenceMatchResult.Matched(new ReferenceValue
            {
                Id = 1, TestId = 1, TestComponentId = 1, Gender = ReferenceValueGender.Both,
                AgeMin = 0, AgeMax = 0, AgeUnit = AgeUnit.Years, NormalRange = "4-10"
            }));

        var service = CreateService();

        var result = await service.ValidateResultAsync(
            1, "5.0", "male", new Age(30, 0, 0), false);

        Assert.Equal(ResultStatus.Normal, result.Status);
        Assert.Equal("4-10", result.ReferenceRange);
    }

    [Fact]
    public async Task ValidateResult_AboveRange_ReturnsHigh()
    {
        SetupResultItem(1, 1);
        _referenceValues
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue>());

        _matcher
            .Setup(m => m.Match(It.IsAny<IReadOnlyList<ReferenceValue>>(), 1, "male", It.IsAny<Age>(), false))
            .Returns(ReferenceMatchResult.Matched(new ReferenceValue
            {
                Id = 1, TestId = 1, TestComponentId = 1, Gender = ReferenceValueGender.Both,
                AgeMin = 0, AgeMax = 0, AgeUnit = AgeUnit.Years, NormalRange = "4-10",
                HighComment = "High value"
            }));

        var service = CreateService();

        var result = await service.ValidateResultAsync(
            1, "15.0", "male", new Age(30, 0, 0), false);

        Assert.Equal(ResultStatus.High, result.Status);
        Assert.Equal("High value", result.WarningComment);
    }

    [Fact]
    public async Task ValidateResult_BelowRange_ReturnsLow()
    {
        SetupResultItem(1, 1);
        _referenceValues
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReferenceValue>());

        _matcher
            .Setup(m => m.Match(It.IsAny<IReadOnlyList<ReferenceValue>>(), 1, "male", It.IsAny<Age>(), false))
            .Returns(ReferenceMatchResult.Matched(new ReferenceValue
            {
                Id = 1, TestId = 1, TestComponentId = 1, Gender = ReferenceValueGender.Both,
                AgeMin = 0, AgeMax = 0, AgeUnit = AgeUnit.Years, NormalRange = "4-10",
                LowComment = "Low value"
            }));

        var service = CreateService();

        var result = await service.ValidateResultAsync(
            1, "2.0", "male", new Age(30, 0, 0), false);

        Assert.Equal(ResultStatus.Low, result.Status);
        Assert.Equal("Low value", result.WarningComment);
    }

    [Fact]
    public async Task ValidateResult_NullResultItem_ReturnsNormal()
    {
        _visitTestResultItems
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VisitTestResultItem?)null);

        var service = CreateService();

        var result = await service.ValidateResultAsync(
            1, "5.0", "male", new Age(30, 0, 0), false);

        Assert.Equal(ResultStatus.Normal, result.Status);
        Assert.Equal(ReferenceMatchKind.NoRangeConfigured, result.MatchKind);
    }
}
