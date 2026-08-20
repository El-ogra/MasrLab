using MediatR;
using MasrLab.Application.Features.FixedComments.Commands.AddCommentTemplate;
using MasrLab.Application.Features.FixedComments.Commands.DeleteCommentTemplate;
using MasrLab.Application.Features.FixedComments.Commands.UpdateCommentTemplate;
using MasrLab.Application.Features.ResultsEntry.Commands.ApplyCommentTemplate;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class CommentTemplateCommandHandlerTests
{
    [Fact]
    public async Task AddCommentTemplate_persists_template_for_test()
    {
        var repository = new Mock<IRepository<CommentTemplate>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        CommentTemplate? added = null;
        repository
            .Setup(x => x.AddAsync(It.IsAny<CommentTemplate>(), It.IsAny<CancellationToken>()))
            .Callback<CommentTemplate, CancellationToken>((template, _) =>
            {
                template.Id = 11;
                added = template;
            });

        var id = await new AddCommentTemplateCommandHandler(repository.Object, unitOfWork.Object)
            .Handle(new(20, "Repeat sample"), CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(11, id);
        Assert.Equal(20, added!.TestId);
        Assert.Equal("Repeat sample", added.Text);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCommentTemplate_requires_template_to_belong_to_test()
    {
        var template = new CommentTemplate { Id = 11, TestId = 20, Text = "Old" };
        var repository = new Mock<IRepository<CommentTemplate>>();
        repository.Setup(x => x.GetByIdAsync(11, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        await Assert.ThrowsAsync<MasrLab.Domain.Exceptions.BusinessRuleViolationException>(() =>
            new UpdateCommentTemplateCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object)
                .Handle(new(11, 21, "New"), CancellationToken.None));

        Assert.Equal("Old", template.Text);
        repository.Verify(x => x.Update(It.IsAny<CommentTemplate>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCommentTemplate_changes_owned_template()
    {
        var template = new CommentTemplate { Id = 11, TestId = 20, Text = "Old" };
        var repository = new Mock<IRepository<CommentTemplate>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByIdAsync(11, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        await new UpdateCommentTemplateCommandHandler(repository.Object, unitOfWork.Object)
            .Handle(new(11, 20, "New"), CancellationToken.None);

        Assert.Equal("New", template.Text);
        repository.Verify(x => x.Update(template), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentTemplate_soft_deletes_owned_template()
    {
        var template = new CommentTemplate { Id = 11, TestId = 20, Text = "Old" };
        var repository = new Mock<IRepository<CommentTemplate>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByIdAsync(11, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        await new DeleteCommentTemplateCommandHandler(repository.Object, unitOfWork.Object)
            .Handle(new(11, 20), CancellationToken.None);

        Assert.True(template.IsDeleted);
        repository.Verify(x => x.Update(template), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplyCommentTemplate_updates_comment_history_and_reprint_flag()
    {
        var testResult = TestResult.Enter(100, "5", 1);
        testResult.Id = 1;
        testResult.PrintCount = 1;
        testResult.SetComment("Old comment");

        var resultItem = new VisitTestResultItem { Id = 100, VisitTestId = 10 };
        var visitTest = new VisitTest(1, 20, 100m, false);
        var visit = PatientVisit.Create(1, 1, "L-1", null, null);
        visit.AddVisitTest(visitTest);
        visit.EnterAllResults();
        visit.MarkAsPrinted();
        var template = new CommentTemplate { Id = 7, TestId = 20, Text = "New comment" };
        TestResultEditHistory? addedHistory = null;

        var results = new Mock<ITestResultRepository>();
        var resultItems = new Mock<IVisitTestResultItemRepository>();
        var visits = new Mock<IVisitRepository>();
        var templates = new Mock<IRepository<CommentTemplate>>();
        var history = new Mock<IRepository<TestResultEditHistory>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        results.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(testResult);
        resultItems.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(resultItem);
        visits.Setup(x => x.GetVisitTestAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(visitTest);
        visits.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        templates.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(template);
        history
            .Setup(x => x.AddAsync(It.IsAny<TestResultEditHistory>(), It.IsAny<CancellationToken>()))
            .Callback<TestResultEditHistory, CancellationToken>((entry, _) => addedHistory = entry);

        await new ApplyCommentTemplateCommandHandler(
                results.Object, resultItems.Object, visits.Object, templates.Object, history.Object, unitOfWork.Object)
            .Handle(new(1, 7, 2), CancellationToken.None);

        Assert.Equal("New comment", testResult.Comment);
        Assert.True(testResult.ReprintRequired);
        Assert.NotNull(addedHistory);
        Assert.Equal(ResultEditChangeType.CommentOnly, addedHistory!.ChangeType);
        Assert.Equal("Old comment", addedHistory.OldComment);
        Assert.Equal("New comment", addedHistory.NewComment);
        Assert.Equal(2, addedHistory.EditedByUserId);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(VisitStatus.Registered)]
    [InlineData(VisitStatus.Closed)]
    public async Task ApplyCommentTemplate_rejects_non_editable_visit_status(VisitStatus status)
    {
        var testResult = TestResult.Enter(100, "5", 1);
        testResult.Id = 1;
        var resultItem = new VisitTestResultItem { Id = 100, VisitTestId = 10 };
        var visitTest = new VisitTest(1, 20, 100m, false);
        var visit = PatientVisit.Create(1, 1, "L-1", null, null);
        visit.AddVisitTest(visitTest);
        if (status == VisitStatus.ResultsEntered)
            visit.EnterAllResults();
        if (status == VisitStatus.Closed)
        {
            visit.EnterAllResults();
            visit.Close(100m);
        }

        var results = new Mock<ITestResultRepository>();
        var resultItems = new Mock<IVisitTestResultItemRepository>();
        var visits = new Mock<IVisitRepository>();
        var templates = new Mock<IRepository<CommentTemplate>>();
        var history = new Mock<IRepository<TestResultEditHistory>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        results.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(testResult);
        resultItems.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(resultItem);
        visits.Setup(x => x.GetVisitTestAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(visitTest);
        visits.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        await Assert.ThrowsAsync<MasrLab.Domain.Exceptions.BusinessRuleViolationException>(() =>
            new ApplyCommentTemplateCommandHandler(
                    results.Object, resultItems.Object, visits.Object, templates.Object, history.Object, unitOfWork.Object)
                .Handle(new(1, 7, 2), CancellationToken.None));

        templates.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
