using MasrLab.Application.Features.ResultsEntry.Commands.EditTestResult;
using MasrLab.Application.Features.ResultsEntry.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;
using Moq;

namespace MasrLab.Application.Tests;

public class EditTestResultCommandHandlerTests
{
    private const int TestResultId = 1;
    private const int VisitTestResultItemId = 100;
    private const int VisitTestId = 10;
    private const int PatientVisitId = 1;
    private const int PatientId = 1;
    private const int EditedByUserId = 2;

    private readonly Mock<ITestResultRepository> _testResultRepository;
    private readonly Mock<IVisitTestResultItemRepository> _visitTestResultItemRepository;
    private readonly Mock<IVisitRepository> _visitRepository;
    private readonly Mock<IPatientRepository> _patientRepository;
    private readonly Mock<IResultValidationService> _resultValidationService;
    private readonly Mock<IVisitCompletionEvaluator> _completionEvaluator;
    private readonly Mock<IRepository<TestResultEditHistory>> _historyRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public EditTestResultCommandHandlerTests()
    {
        _testResultRepository = new Mock<ITestResultRepository>();
        _visitTestResultItemRepository = new Mock<IVisitTestResultItemRepository>();
        _visitRepository = new Mock<IVisitRepository>();
        _patientRepository = new Mock<IPatientRepository>();
        _resultValidationService = new Mock<IResultValidationService>();
        _completionEvaluator = new Mock<IVisitCompletionEvaluator>();
        _historyRepository = new Mock<IRepository<TestResultEditHistory>>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private EditTestResultCommandHandler CreateHandler()
        => new(
            _testResultRepository.Object,
            _visitTestResultItemRepository.Object,
            _visitRepository.Object,
            _patientRepository.Object,
            _resultValidationService.Object,
            _completionEvaluator.Object,
            _historyRepository.Object,
            _unitOfWork.Object);

    private static PatientVisit CreateResultsEnteredVisit(int patientId = PatientId)
    {
        var visit = PatientVisit.Create(patientId, 1, "L-1", null, null);
        visit.AddVisitTest(new VisitTest(visit.Id, 20, 100m, false));
        visit.EnterAllResults();
        return visit;
    }

    private void SetupFullChain(
        TestResult testResult,
        PatientVisit visit,
        Patient? patient = null)
    {
        patient ??= new Patient
        {
            Id = visit.PatientId,
            Name = "Ahmed",
            Gender = Gender.Male,
            Age = new Age(30, 0, 0),
            Pregnancy = false
        };

        _testResultRepository
            .Setup(r => r.GetByIdAsync(TestResultId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testResult);

        _visitTestResultItemRepository
            .Setup(r => r.GetByIdAsync(testResult.VisitTestResultItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VisitTestResultItem
            {
                Id = testResult.VisitTestResultItemId,
                VisitTestId = VisitTestId,
                SourceTestComponentId = 1,
                ComponentName = "Test",
                ComponentUnit = "Unit",
                DisplayOrder = 1,
                ResultEntryKind = ResultEntryKind.Ordinary
            });

        _visitRepository
            .Setup(r => r.GetVisitTestAsync(VisitTestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VisitTest(PatientVisitId, 20, 100m, false));

        _visitRepository
            .Setup(r => r.GetByIdAsync(PatientVisitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);

        _patientRepository
            .Setup(r => r.GetByIdAsync(visit.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
    }

    private void SetupValidation(ResultStatus status = ResultStatus.Normal, string? referenceRange = "1-10",
        string? warningComment = null)
    {
        _resultValidationService
            .Setup(s => s.ValidateResultAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultValidationOutput
            {
                Status = status,
                ReferenceRange = referenceRange,
                WarningComment = warningComment,
                MatchKind = ReferenceMatchKind.Matched
            });
    }

    private static EditTestResultCommand ValueOnlyCommand(string newValue = "7.0")
        => new(TestResultId, EditedByUserId, newValue, null);

    private static EditTestResultCommand CommentOnlyCommand(string newComment = "new comment")
        => new(TestResultId, EditedByUserId, null,
            new CommentPatch { UpdateComment = true, NewComment = newComment });

    private static EditTestResultCommand ValueAndCommentCommand(
        string newValue = "7.0", string newComment = "new comment")
        => new(TestResultId, EditedByUserId, newValue,
            new CommentPatch { UpdateComment = true, NewComment = newComment });

    [Fact]
    public async Task ValueEdit_RecalculatesStatusAndReferenceRange()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);
        SetupValidation(ResultStatus.High, "4-8");

        await CreateHandler().Handle(ValueOnlyCommand("7.0"), CancellationToken.None);

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                VisitTestResultItemId, "7.0", "male",
                It.IsAny<Age>(), false, It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(ResultStatus.High, testResult.Status);
        Assert.Equal("4-8", testResult.ReferenceRange);
    }

    [Fact]
    public async Task CommentOnlyEdit_DoesNotRecalculateStatus()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        testResult.Status = ResultStatus.Normal;
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);

        await CreateHandler().Handle(CommentOnlyCommand("updated comment"), CancellationToken.None);

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.Equal(ResultStatus.Normal, testResult.Status);
    }

    [Fact]
    public async Task ValueAndComment_ProducesValueAndCommentChangeType()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);
        SetupValidation();

        await CreateHandler().Handle(ValueAndCommentCommand(), CancellationToken.None);

        var editedEvent = testResult.DomainEvents
            .OfType<TestResultEdited>()
            .Single(e => e.ChangeType == ResultEditChangeType.ValueAndComment);
        Assert.Equal("5.0", editedEvent.OldValue);
        Assert.Equal("7.0", editedEvent.NewValue);
    }

    [Fact]
    public async Task CorrectOldNewValueSnapshots()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);
        SetupValidation();

        await CreateHandler().Handle(ValueOnlyCommand("7.0"), CancellationToken.None);

        var editedEvent = testResult.DomainEvents
            .OfType<TestResultEdited>()
            .Single();
        Assert.Equal("5.0", editedEvent.OldValue);
        Assert.Equal("7.0", editedEvent.NewValue);
    }

    [Fact]
    public async Task CorrectOldNewCommentSnapshots()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        testResult.SetComment("old");
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);

        await CreateHandler().Handle(CommentOnlyCommand("new"), CancellationToken.None);

        var editedEvent = testResult.DomainEvents
            .OfType<TestResultEdited>()
            .Single();
        Assert.Equal("old", editedEvent.OldComment);
        Assert.Equal("new", editedEvent.NewComment);
    }

    [Fact]
    public async Task PrePrintEdit_RequiresEdit()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        testResult.PrintCount = 0;
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);
        SetupValidation();

        await CreateHandler().Handle(ValueOnlyCommand("7.0"), CancellationToken.None);

        Assert.Equal("7.0", testResult.Value);
    }

    [Fact]
    public async Task PostPrintEditValue_RequiresEditPrinted()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        testResult.PrintCount = 1;
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(ValueOnlyCommand("7.0"), CancellationToken.None));

        Assert.Equal("5.0", testResult.Value);
    }

    [Fact]
    public async Task PostPrintEdit_SetsReprintRequired()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        testResult.SetComment("old");
        testResult.PrintCount = 1;
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);

        await CreateHandler().Handle(CommentOnlyCommand("new"), CancellationToken.None);

        Assert.True(testResult.ReprintRequired);
    }

    [Fact]
    public async Task NoOpEdit_IsRejected()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(ValueOnlyCommand("5.0"), CancellationToken.None));
    }

    [Fact]
    public async Task TestResultEditHistory_WrittenAtomically()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);
        SetupValidation();

        await CreateHandler().Handle(ValueOnlyCommand("7.0"), CancellationToken.None);

        _historyRepository.Verify(
            r => r.AddAsync(It.IsAny<TestResultEditHistory>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompletionEvaluator_InvokedAfterValueEdit()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);
        SetupValidation();

        await CreateHandler().Handle(ValueOnlyCommand("7.0"), CancellationToken.None);

        _completionEvaluator.Verify(
            e => e.EvaluateAsync(visit, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValueEdit_HistoryHasCorrectOldAndNewValues()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);
        SetupValidation();

        await CreateHandler().Handle(ValueOnlyCommand("7.0"), CancellationToken.None);

        TestResultEditHistory? capturedHistory = null;
        _historyRepository
            .Setup(r => r.AddAsync(It.IsAny<TestResultEditHistory>(), It.IsAny<CancellationToken>()))
            .Callback<TestResultEditHistory, CancellationToken>((h, _) => capturedHistory = h);

        // Re-run to capture the history object
        var testResult2 = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        var visit2 = CreateResultsEnteredVisit();
        SetupFullChain(testResult2, visit2);
        SetupValidation();
        await CreateHandler().Handle(ValueOnlyCommand("7.0"), CancellationToken.None);

        Assert.NotNull(capturedHistory);
        Assert.Equal("5.0", capturedHistory!.OldValue);
        Assert.Equal("7.0", capturedHistory.NewValue);
        Assert.Null(capturedHistory.OldComment);
        Assert.Null(capturedHistory.NewComment);
        Assert.Equal(ResultEditChangeType.ValueOnly, capturedHistory.ChangeType);
    }

    [Fact]
    public async Task CommentOnlyEdit_HistoryHasCorrectOldAndNewComments()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        testResult.SetComment("old comment");
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);

        TestResultEditHistory? capturedHistory = null;
        _historyRepository
            .Setup(r => r.AddAsync(It.IsAny<TestResultEditHistory>(), It.IsAny<CancellationToken>()))
            .Callback<TestResultEditHistory, CancellationToken>((h, _) => capturedHistory = h);

        await CreateHandler().Handle(CommentOnlyCommand("new comment"), CancellationToken.None);

        Assert.NotNull(capturedHistory);
        Assert.Null(capturedHistory!.OldValue);
        Assert.Null(capturedHistory.NewValue);
        Assert.Equal("old comment", capturedHistory.OldComment);
        Assert.Equal("new comment", capturedHistory.NewComment);
        Assert.Equal(ResultEditChangeType.CommentOnly, capturedHistory.ChangeType);
    }

    [Fact]
    public async Task ValueAndCommentEdit_HistoryHasAllFourFields()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        testResult.SetComment("old comment");
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);
        SetupValidation();

        TestResultEditHistory? capturedHistory = null;
        _historyRepository
            .Setup(r => r.AddAsync(It.IsAny<TestResultEditHistory>(), It.IsAny<CancellationToken>()))
            .Callback<TestResultEditHistory, CancellationToken>((h, _) => capturedHistory = h);

        await CreateHandler().Handle(ValueAndCommentCommand("7.0", "new comment"), CancellationToken.None);

        Assert.NotNull(capturedHistory);
        Assert.Equal("5.0", capturedHistory!.OldValue);
        Assert.Equal("7.0", capturedHistory.NewValue);
        Assert.Equal("old comment", capturedHistory.OldComment);
        Assert.Equal("new comment", capturedHistory.NewComment);
        Assert.Equal(ResultEditChangeType.ValueAndComment, capturedHistory.ChangeType);
    }

    [Fact]
    public async Task CommentOnlyEdit_StatusRemainsUnchanged()
    {
        var testResult = TestResult.Enter(VisitTestResultItemId, "5.0", 1);
        testResult.Status = ResultStatus.Normal;
        testResult.SetComment("old");
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(testResult, visit);

        await CreateHandler().Handle(CommentOnlyCommand("new"), CancellationToken.None);

        Assert.Equal(ResultStatus.Normal, testResult.Status);
        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
