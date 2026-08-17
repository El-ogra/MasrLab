using MasrLab.Application.Features.ResultsEntry.Commands.ReapplyReferenceValues;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;
using MediatR;
using Moq;

namespace MasrLab.Application.Tests;

public class ReapplyReferenceValuesCommandHandlerTests
{
    private const int TestResultId = 1;
    private const int VisitTestResultItemId = 100;
    private const int VisitTestId = 10;
    private const int PatientVisitId = 1;
    private const int PatientId = 1;
    private const int AppliedByUserId = 2;

    private readonly Mock<ITestResultRepository> _testResultRepository = new();
    private readonly Mock<IVisitTestResultItemRepository> _visitTestResultItemRepository = new();
    private readonly Mock<IVisitRepository> _visitRepository = new();
    private readonly Mock<IPatientRepository> _patientRepository = new();
    private readonly Mock<IResultValidationService> _resultValidationService = new();
    private readonly Mock<IReferenceValueRepository> _referenceValueRepository = new();
    private readonly Mock<IRepository<TestResultEditHistory>> _historyRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private ReapplyReferenceValuesCommandHandler CreateHandler()
        => new(
            _testResultRepository.Object,
            _visitTestResultItemRepository.Object,
            _visitRepository.Object,
            _patientRepository.Object,
            _resultValidationService.Object,
            _referenceValueRepository.Object,
            new ReferenceValueMatcher(),
            _historyRepository.Object,
            _unitOfWork.Object);

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

    private void SetupValidation(
        ResultStatus status = ResultStatus.Normal,
        string? referenceRange = "2-8",
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

    private static PatientVisit CreateResultsEnteredVisit()
    {
        var visit = PatientVisit.Create(PatientId, 1, "L-1", null, null);
        visit.AddVisitTest(new VisitTest(visit.Id, 20, 100m, false));
        visit.EnterAllResults();
        return visit;
    }

    private static TestResult CreateResult(string? comment = "old high")
    {
        var result = TestResult.Enter(VisitTestResultItemId, "12", 1);
        result.Id = TestResultId;
        result.ReferenceRange = "1-10";
        result.Status = ResultStatus.High;
        if (comment is not null)
            result.SetComment(comment);
        return result;
    }

    private void SetupPreviousHighComment(string comment = "old high")
    {
        _referenceValueRepository
            .Setup(r => r.GetByTestComponentIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new ReferenceValue
                {
                    Id = 1,
                    TestComponentId = 1,
                    Gender = ReferenceValueGender.Both,
                    AgeUnit = AgeUnit.Years,
                    NormalRange = "1-10",
                    AgeMin = 0,
                    AgeMax = 0,
                    HighComment = comment
                }
            });
    }

    [Fact]
    public async Task Reapply_UpdatesStatusRangeAndAutomaticComment()
    {
        var result = CreateResult();
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(result, visit);
        SetupPreviousHighComment();
        SetupValidation(ResultStatus.Low, "2-8", "new low");

        await CreateHandler().Handle(
            new ReapplyReferenceValuesCommand(TestResultId, AppliedByUserId),
            CancellationToken.None);

        Assert.Equal(ResultStatus.Low, result.Status);
        Assert.Equal("2-8", result.ReferenceRange);
        Assert.Equal("new low", result.Comment);
        _historyRepository.Verify(
            r => r.AddAsync(It.Is<TestResultEditHistory>(h =>
                h.OldValue == null && h.NewValue == null &&
                h.OldComment == "old high" && h.NewComment == "new low" &&
                h.ChangeType == ResultEditChangeType.ReferenceReapplied),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reapply_ProtectsManualComment()
    {
        var result = CreateResult("manual comment");
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(result, visit);
        SetupPreviousHighComment();
        SetupValidation(ResultStatus.Normal, "2-8", null);

        await CreateHandler().Handle(
            new ReapplyReferenceValuesCommand(TestResultId, AppliedByUserId),
            CancellationToken.None);

        Assert.Equal("manual comment", result.Comment);
        Assert.Equal(ResultStatus.Normal, result.Status);
        Assert.Equal("2-8", result.ReferenceRange);
        _historyRepository.Verify(
            r => r.AddAsync(It.Is<TestResultEditHistory>(h =>
                h.OldComment == null && h.NewComment == null &&
                h.ChangeType == ResultEditChangeType.ReferenceReapplied),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Reapply_MarksPrintedResultForReprint()
    {
        var result = CreateResult();
        result.PrintCount = 1;
        var visit = CreateResultsEnteredVisit();
        SetupFullChain(result, visit);
        SetupPreviousHighComment();
        SetupValidation(ResultStatus.High, "2-8", "new high");

        await CreateHandler().Handle(
            new ReapplyReferenceValuesCommand(TestResultId, AppliedByUserId),
            CancellationToken.None);

        Assert.True(result.ReprintRequired);
    }

    [Fact]
    public async Task Reapply_UsesStoredValueAndPatientAge()
    {
        var result = CreateResult();
        var visit = CreateResultsEnteredVisit();
        var patient = new Patient
        {
            Id = PatientId,
            Name = "Ahmed",
            Gender = Gender.Male,
            Age = new Age(1, AgeUnit.Months),
            Pregnancy = false
        };
        SetupFullChain(result, visit, patient);
        SetupPreviousHighComment();
        SetupValidation();

        await CreateHandler().Handle(
            new ReapplyReferenceValuesCommand(TestResultId, AppliedByUserId),
            CancellationToken.None);

        _resultValidationService.Verify(s => s.ValidateResultAsync(
            VisitTestResultItemId,
            "12",
            "male",
            It.Is<Age>(age => age.RecordedUnit == AgeUnit.Months && age.TotalMonths == 1),
            false,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reapply_RequiresResultsEnteredVisit()
    {
        var result = CreateResult();
        var visit = PatientVisit.Create(PatientId, 1, "L-1", null, null);
        SetupFullChain(result, visit);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            CreateHandler().Handle(
                new ReapplyReferenceValuesCommand(TestResultId, AppliedByUserId),
                CancellationToken.None));

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
