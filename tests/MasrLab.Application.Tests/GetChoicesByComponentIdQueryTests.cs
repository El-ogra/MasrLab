using MasrLab.Application.Features.ResultsEntry.Queries.GetChoicesByComponentId;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetChoicesByComponentIdQueryTests
{
    private readonly Mock<ITestComponentChoiceRepository> _repository;

    public GetChoicesByComponentIdQueryTests()
    {
        _repository = new Mock<ITestComponentChoiceRepository>();
    }

    private GetChoicesByComponentIdQueryHandler CreateHandler()
        => new(_repository.Object);

    [Fact]
    public async Task Handle_ReturnsActiveChoicesOrderedByDisplayOrder()
    {
        var choices = new List<TestComponentChoice>
        {
            new() { Id = 1, TestComponentId = 1, Value = "Positive", DisplayOrder = 1, IsActive = true },
            new() { Id = 2, TestComponentId = 1, Value = "Equivocal", DisplayOrder = 2, IsActive = true },
            new() { Id = 3, TestComponentId = 1, Value = "Negative", DisplayOrder = 3, IsActive = true }
        };

        _repository
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(choices);

        var result = await CreateHandler().Handle(
            new GetChoicesByComponentIdQuery(1), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("Positive", result[0].Value);
        Assert.Equal("Equivocal", result[1].Value);
        Assert.Equal("Negative", result[2].Value);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoChoicesForComponent()
    {
        _repository
            .Setup(r => r.GetByTestComponentIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestComponentChoice>());

        var result = await CreateHandler().Handle(
            new GetChoicesByComponentIdQuery(99), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MapsToDtoCorrectly()
    {
        var choices = new List<TestComponentChoice>
        {
            new() { Id = 1, TestComponentId = 1, Value = "Yes", DisplayOrder = 1, IsActive = true }
        };

        _repository
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(choices);

        var result = await CreateHandler().Handle(
            new GetChoicesByComponentIdQuery(1), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(1, dto.Id);
        Assert.Equal("Yes", dto.Value);
        Assert.Equal(1, dto.DisplayOrder);
    }

    [Fact]
    public async Task Handle_ExcludesInactiveChoices()
    {
        var choices = new List<TestComponentChoice>
        {
            new() { Id = 1, TestComponentId = 1, Value = "Active", DisplayOrder = 1, IsActive = true }
        };

        _repository
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(choices);

        var result = await CreateHandler().Handle(
            new GetChoicesByComponentIdQuery(1), CancellationToken.None);

        Assert.Single(result);
    }
}
