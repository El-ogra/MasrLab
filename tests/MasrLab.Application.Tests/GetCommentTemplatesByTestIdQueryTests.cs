using MasrLab.Application.Features.ResultsEntry.Queries.GetCommentTemplatesByTestId;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetCommentTemplatesByTestIdQueryTests
{
    private readonly Mock<IRepository<CommentTemplate>> _repository;

    public GetCommentTemplatesByTestIdQueryTests()
    {
        _repository = new Mock<IRepository<CommentTemplate>>();
    }

    private GetCommentTemplatesByTestIdQueryHandler CreateHandler()
        => new(_repository.Object);

    [Fact]
    public async Task Handle_ReturnsTemplatesForGivenTestId()
    {
        var templates = new List<CommentTemplate>
        {
            new() { Id = 1, TestId = 10, Text = "Template A" },
            new() { Id = 2, TestId = 10, Text = "Template B" },
            new() { Id = 3, TestId = 20, Text = "Other Test" }
        };

        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await CreateHandler().Handle(
            new GetCommentTemplatesByTestIdQuery(10), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(10, t.TestId));
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoTemplatesForTest()
    {
        var templates = new List<CommentTemplate>
        {
            new() { Id = 1, TestId = 20, Text = "Other" }
        };

        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await CreateHandler().Handle(
            new GetCommentTemplatesByTestIdQuery(10), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ExcludesSoftDeletedTemplates()
    {
        var templates = new List<CommentTemplate>
        {
            new() { Id = 1, TestId = 10, Text = "Active" },
            new() { Id = 2, TestId = 10, Text = "Deleted" }
        };
        templates[1].IsDeleted = true;

        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await CreateHandler().Handle(
            new GetCommentTemplatesByTestIdQuery(10), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Active", result[0].Text);
    }

    [Fact]
    public async Task Handle_MapsToDtoCorrectly()
    {
        var templates = new List<CommentTemplate>
        {
            new() { Id = 5, TestId = 10, Text = "Check CRP levels" }
        };

        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await CreateHandler().Handle(
            new GetCommentTemplatesByTestIdQuery(10), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(5, dto.Id);
        Assert.Equal(10, dto.TestId);
        Assert.Equal("Check CRP levels", dto.Text);
    }
}
