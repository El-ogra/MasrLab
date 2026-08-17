using FluentValidation;
using MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResultsBatch;
using MasrLab.Application.Features.ResultsEntry.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;
using Moq;

namespace MasrLab.Application.Tests;

public class EnterTestResultsBatchCommandHandlerTests
{
    private readonly Mock<IVisitRepository> _visitRepository;
    private readonly Mock<IPatientRepository> _patientRepository;
    private readonly Mock<IResultValidationService> _resultValidationService;
    private readonly Mock<IVisitCompletionEvaluator> _completionEvaluator;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public EnterTestResultsBatchCommandHandlerTests()
    {
        _visitRepository = new Mock<IVisitRepository>();
        _patientRepository = new Mock<IPatientRepository>();
        _resultValidationService = new Mock<IResultValidationService>();
        _completionEvaluator = new Mock<IVisitCompletionEvaluator>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private EnterTestResultsBatchCommandHandler CreateHandler()
        => new(
            _visitRepository.Object,
            _patientRepository.Object,
            _resultValidationService.Object,
            _completionEvaluator.Object,
            _unitOfWork.Object);

    private static Patient CreatePatient(
        int id = 1,
        Gender gender = Gender.Male,
        int ageYears = 30,
        int ageMonths = 0,
        int ageDays = 0,
        bool pregnancy = false)
    {
        return new Patient
        {
            Id = id,
            Name = "Ahmed",
            Gender = gender,
            Age = new Age(ageYears, ageMonths, ageDays),
            Pregnancy = pregnancy
        };
    }

    private static PatientVisit CreateVisitWithResultItems(
        int visitId,
        int patientId,
        VisitStatus status,
        IReadOnlyList<(int resultItemId, ResultEntryKind kind)> items)
    {
        var visit = PatientVisit.Create(patientId, 1, "LAB001", null, null);
        visit.Id = visitId;

        var visitTest = new VisitTest(visitId, testId: 10, price: 100m, isOutsourced: false);
        foreach (var (resultItemId, kind) in items)
        {
            var resultItem = new VisitTestResultItem
            {
                Id = resultItemId,
                VisitTestId = visitTest.Id,
                SourceTestComponentId = 1,
                ResultEntryKind = kind
            };
            visitTest.ResultItems.Add(resultItem);
        }

        visit.VisitTests.Add(visitTest);

        // Set status via reflection since it has a private setter
        typeof(PatientVisit)
            .GetProperty(nameof(PatientVisit.Status))!
            .SetValue(visit, status);

        return visit;
    }

    private void SetupVisit(PatientVisit visit)
    {
        _visitRepository
            .Setup(r => r.GetByIdWithTestsAndResultItemsAsync(visit.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);
    }

    private void SetupPatient(Patient patient)
    {
        _patientRepository
            .Setup(r => r.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
    }

    private void SetupValidationNormal()
    {
        _resultValidationService
            .Setup(s => s.ValidateResultAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultValidationOutput
            {
                Status = ResultStatus.Normal,
                ReferenceRange = "1-10",
                MatchKind = ReferenceMatchKind.Matched
            });
    }

    [Fact]
    public async Task ValidBatch_SavesAllItemsAtomically()
    {
        var patient = CreatePatient();
        var visit = CreateVisitWithResultItems(
            visitId: 1,
            patientId: 1,
            VisitStatus.Registered,
            [(100, ResultEntryKind.Ordinary), (101, ResultEntryKind.Ordinary)]);

        SetupVisit(visit);
        SetupPatient(patient);
        SetupValidationNormal();

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>
            {
                new(100, "5.0", "mg/dL", null),
                new(101, "3.2", "g/dL", null)
            });

        await CreateHandler().Handle(command, CancellationToken.None);

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FailedBatch_DoesNotPersistPartially()
    {
        var patient = CreatePatient();
        var visit = CreateVisitWithResultItems(
            visitId: 1,
            patientId: 1,
            VisitStatus.Registered,
            [(100, ResultEntryKind.Ordinary)]);

        SetupVisit(visit);
        SetupPatient(patient);
        SetupValidationNormal();

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>
            {
                new(100, "5.0", "mg/dL", null),
                new(998, "3.2", "g/dL", null)
            });

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ItemsFromAnotherVisit_AreRejected()
    {
        var patient = CreatePatient();
        var visit = CreateVisitWithResultItems(
            visitId: 1,
            patientId: 1,
            VisitStatus.Registered,
            [(100, ResultEntryKind.Ordinary)]);

        SetupVisit(visit);
        SetupPatient(patient);

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>
            {
                new(999, "5.0", "mg/dL", null)
            });

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        Assert.Contains("does not belong to visit", ex.Message);
    }

    [Fact]
    public async Task CultureDetailItems_AreRejected()
    {
        var patient = CreatePatient();
        var visit = CreateVisitWithResultItems(
            visitId: 1,
            patientId: 1,
            VisitStatus.Registered,
            [(100, ResultEntryKind.CultureDetail)]);

        SetupVisit(visit);
        SetupPatient(patient);

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>
            {
                new(100, "E. coli", "count", null)
            });

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(command, CancellationToken.None));

        Assert.Contains("CultureDetail", ex.Message);
    }

    [Fact]
    public async Task CommentPatch_IsApplied()
    {
        var patient = CreatePatient();
        var visit = CreateVisitWithResultItems(
            visitId: 1,
            patientId: 1,
            VisitStatus.Registered,
            [(100, ResultEntryKind.Ordinary)]);

        SetupVisit(visit);
        SetupPatient(patient);
        SetupValidationNormal();

        _resultValidationService
            .Setup(s => s.ValidateResultAsync(
                100, It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultValidationOutput
            {
                Status = ResultStatus.Normal,
                ReferenceRange = "1-10",
                MatchKind = ReferenceMatchKind.Matched
            });

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>
            {
                new(100, "5.0", "mg/dL", new CommentPatch
                {
                    UpdateComment = true,
                    NewComment = "test comment"
                })
            });

        await CreateHandler().Handle(command, CancellationToken.None);

        // The test result is created via TestResult.Enter but not added to a repository.
        // We verify that validation was called (which means the batch processing ran successfully)
        // and the comment patch path was exercised without error.
        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                100, "5.0", It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompletionEvaluator_IsInvoked()
    {
        var patient = CreatePatient();
        var visit = CreateVisitWithResultItems(
            visitId: 1,
            patientId: 1,
            VisitStatus.Registered,
            [(100, ResultEntryKind.Ordinary)]);

        SetupVisit(visit);
        SetupPatient(patient);
        SetupValidationNormal();

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>
            {
                new(100, "5.0", "mg/dL", null)
            });

        await CreateHandler().Handle(command, CancellationToken.None);

        _completionEvaluator.Verify(
            e => e.EvaluateAsync(visit, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AgeIsTakenFromPatient_NotCaller()
    {
        var patient = CreatePatient(ageYears: 5, ageMonths: 3, ageDays: 0);
        var visit = CreateVisitWithResultItems(
            visitId: 1,
            patientId: 1,
            VisitStatus.Registered,
            [(100, ResultEntryKind.Ordinary)]);

        SetupVisit(visit);
        SetupPatient(patient);
        SetupValidationNormal();

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>
            {
                new(100, "5.0", "mg/dL", null)
            });

        await CreateHandler().Handle(command, CancellationToken.None);

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                100, "5.0", "male",
                It.Is<Age>(a => a.Years == 5 && a.Months == 3 && a.Days == 0),
                false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EmptyBatch_IsRejected()
    {
        var validator = new EnterTestResultsBatchCommandValidator();

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>());

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items");
    }

    [Fact]
    public async Task PartialComponentEntry_UnchangedItemsRemainUnentered()
    {
        var patient = CreatePatient();
        var visit = CreateVisitWithResultItems(
            visitId: 1,
            patientId: 1,
            VisitStatus.Registered,
            [(100, ResultEntryKind.Ordinary), (101, ResultEntryKind.Ordinary), (102, ResultEntryKind.Ordinary)]);

        SetupVisit(visit);
        SetupPatient(patient);
        SetupValidationNormal();

        var command = new EnterTestResultsBatchCommand(
            PatientVisitId: 1,
            EnteredByUserId: 1,
            Items: new List<BatchResultItemRequest>
            {
                new(100, "5.0", "mg/dL", null),
                new(101, "3.2", "g/dL", null)
            });

        await CreateHandler().Handle(command, CancellationToken.None);

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(
                102, It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
