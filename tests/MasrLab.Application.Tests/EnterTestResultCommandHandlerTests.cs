using MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class EnterTestResultCommandHandlerTests
{
    private readonly Mock<ITestResultRepository> _testResultRepository;
    private readonly Mock<IResultValidationService> _resultValidationService;
    private readonly Mock<IMedicalHistoryService> _medicalHistoryService;
    private readonly Mock<IVisitRepository> _visitRepository;
    private readonly Mock<ISampleTrackingService> _sampleTrackingService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPatientRepository> _patientRepository;

    public EnterTestResultCommandHandlerTests()
    {
        _testResultRepository = new Mock<ITestResultRepository>();
        _resultValidationService = new Mock<IResultValidationService>();
        _medicalHistoryService = new Mock<IMedicalHistoryService>();
        _visitRepository = new Mock<IVisitRepository>();
        _sampleTrackingService = new Mock<ISampleTrackingService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _patientRepository = new Mock<IPatientRepository>();
    }

    private EnterTestResultCommandHandler CreateHandler()
        => new(
            _testResultRepository.Object,
            _resultValidationService.Object,
            _medicalHistoryService.Object,
            _visitRepository.Object,
            _sampleTrackingService.Object,
            _unitOfWork.Object,
            _patientRepository.Object);

    private static EnterTestResultCommand CreateCommand(string? overrideReason = null)
        => new(
            VisitTestId: 100,
            Value: "5.0",
            Unit: "cells/uL",
            ReferenceRange: "1-10",
            Status: ResultStatus.Normal,
            EnteredByUserId: 1,
            PatientId: 1,
            AgeYears: 30,
            OverrideReason: overrideReason);

    private void SetupPatient(Gender gender = Gender.Male)
    {
        _patientRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Id = 1, Name = "Ahmed", Gender = gender });
    }

    private void SetupVisitTest()
    {
        _visitRepository
            .Setup(r => r.GetVisitTestAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VisitTest(patientVisitId: 10, testId: 20, price: 100m, isOutsourced: false));
    }

    private void SetupValidationNormal()
    {
        _resultValidationService
            .Setup(s => s.ValidateResultAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultStatus.Normal);
    }

    [Fact]
    public async Task Handle_WhenSampleCollected_EntersResultNormally()
    {
        SetupPatient();
        SetupVisitTest();
        SetupValidationNormal();
        _sampleTrackingService
            .Setup(s => s.IsSampleCollectedAsync(10, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        TestResult? captured = null;
        _testResultRepository
            .Setup(r => r.AddAsync(It.IsAny<TestResult>(), It.IsAny<CancellationToken>()))
            .Callback<TestResult, CancellationToken>((tr, _) => captured = tr);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Null(captured.OverrideReason);
        _testResultRepository.Verify(r => r.AddAsync(It.IsAny<TestResult>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSampleNotCollectedAndNoOverride_ThrowsAndDoesNotAdd()
    {
        SetupPatient();
        SetupVisitTest();
        _sampleTrackingService
            .Setup(s => s.IsSampleCollectedAsync(10, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(CreateCommand(overrideReason: null), CancellationToken.None));

        _testResultRepository.Verify(r => r.AddAsync(It.IsAny<TestResult>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSampleNotCollectedWithOverrideReason_EntersAndRecordsReason()
    {
        SetupPatient();
        SetupVisitTest();
        SetupValidationNormal();
        _sampleTrackingService
            .Setup(s => s.IsSampleCollectedAsync(10, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        TestResult? captured = null;
        _testResultRepository
            .Setup(r => r.AddAsync(It.IsAny<TestResult>(), It.IsAny<CancellationToken>()))
            .Callback<TestResult, CancellationToken>((tr, _) => captured = tr);

        await CreateHandler().Handle(CreateCommand(overrideReason: "reserve sample batch 7"), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("reserve sample batch 7", captured.OverrideReason);
        _testResultRepository.Verify(r => r.AddAsync(It.IsAny<TestResult>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSampleNotCollectedWithBlankOverrideReason_Throws()
    {
        SetupPatient();
        SetupVisitTest();
        _sampleTrackingService
            .Setup(s => s.IsSampleCollectedAsync(10, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(CreateCommand(overrideReason: "   "), CancellationToken.None));

        _testResultRepository.Verify(r => r.AddAsync(It.IsAny<TestResult>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPatientHasFemaleGender_UsesPatientGenderInValidation()
    {
        SetupPatient(Gender.Female);
        SetupVisitTest();
        SetupValidationNormal();
        _sampleTrackingService
            .Setup(s => s.IsSampleCollectedAsync(10, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(100, "5.0", "female", 30, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ThrowsBeforeAnyValidation()
    {
        SetupVisitTest();
        _patientRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);
        _sampleTrackingService
            .Setup(s => s.IsSampleCollectedAsync(10, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(CreateCommand(), CancellationToken.None));

        _resultValidationService.Verify(
            s => s.ValidateResultAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _testResultRepository.Verify(r => r.AddAsync(It.IsAny<TestResult>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
