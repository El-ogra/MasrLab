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

    public EnterTestResultCommandHandlerTests()
    {
        _testResultRepository = new Mock<ITestResultRepository>();
        _resultValidationService = new Mock<IResultValidationService>();
        _medicalHistoryService = new Mock<IMedicalHistoryService>();
        _visitRepository = new Mock<IVisitRepository>();
        _sampleTrackingService = new Mock<ISampleTrackingService>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private EnterTestResultCommandHandler CreateHandler()
        => new(
            _testResultRepository.Object,
            _resultValidationService.Object,
            _medicalHistoryService.Object,
            _visitRepository.Object,
            _sampleTrackingService.Object,
            _unitOfWork.Object);

    private static EnterTestResultCommand CreateCommand(string? overrideReason = null)
        => new(
            VisitTestId: 100,
            Value: "5.0",
            Unit: "cells/uL",
            ReferenceRange: "1-10",
            Status: ResultStatus.Normal,
            EnteredByUserId: 1,
            PatientId: 1,
            Gender: "male",
            AgeYears: 30,
            OverrideReason: overrideReason);

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
        SetupVisitTest();
        _sampleTrackingService
            .Setup(s => s.IsSampleCollectedAsync(10, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(CreateCommand(overrideReason: "   "), CancellationToken.None));

        _testResultRepository.Verify(r => r.AddAsync(It.IsAny<TestResult>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
