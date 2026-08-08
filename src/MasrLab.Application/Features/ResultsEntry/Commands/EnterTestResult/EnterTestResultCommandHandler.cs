using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public class EnterTestResultCommandHandler : IRequestHandler<EnterTestResultCommand, Unit>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IResultValidationService _resultValidationService;
    private readonly IMedicalHistoryService _medicalHistoryService;
    private readonly IVisitRepository _visitRepository;
    private readonly ISampleTrackingService _sampleTrackingService;
    private readonly IUnitOfWork _unitOfWork;

    public EnterTestResultCommandHandler(
        ITestResultRepository testResultRepository,
        IResultValidationService resultValidationService,
        IMedicalHistoryService medicalHistoryService,
        IVisitRepository visitRepository,
        ISampleTrackingService sampleTrackingService,
        IUnitOfWork unitOfWork)
    {
        _testResultRepository = testResultRepository;
        _resultValidationService = resultValidationService;
        _medicalHistoryService = medicalHistoryService;
        _visitRepository = visitRepository;
        _sampleTrackingService = sampleTrackingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(EnterTestResultCommand request, CancellationToken cancellationToken)
    {
        var visitTest = await _visitRepository.GetVisitTestAsync(request.VisitTestId, cancellationToken)
            ?? throw new InvalidOperationException($"VisitTest with Id {request.VisitTestId} not found.");

        var isSampleCollected = await _sampleTrackingService.IsSampleCollectedAsync(
            visitTest.PatientVisitId,
            visitTest.TestId,
            cancellationToken);

        if (!isSampleCollected && string.IsNullOrWhiteSpace(request.OverrideReason))
        {
            throw new BusinessRuleViolationException(
                "Cannot enter a result for a test whose sample has not been collected. " +
                "Provide an override reason to proceed.");
        }

        var status = await _resultValidationService.ValidateResultAsync(
            request.VisitTestId,
            request.Value,
            request.Gender,
            request.AgeYears,
            cancellationToken);

        var testResult = TestResult.Enter(request.VisitTestId, request.Value, request.EnteredByUserId);

        testResult.Unit = request.Unit;
        testResult.ReferenceRange = request.ReferenceRange;
        testResult.Status = status;
        testResult.OverrideReason = request.OverrideReason;

        await _testResultRepository.AddAsync(testResult, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _medicalHistoryService.ShouldAutoInsertHistoryAsync(request.PatientId, request.VisitTestId, cancellationToken);

        return Unit.Value;
    }
}
