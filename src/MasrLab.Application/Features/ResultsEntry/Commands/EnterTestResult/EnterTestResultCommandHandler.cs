using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public class EnterTestResultCommandHandler : IRequestHandler<EnterTestResultCommand, Unit>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IResultValidationService _resultValidationService;
    private readonly IMedicalHistoryService _medicalHistoryService;
    private readonly IVisitRepository _visitRepository;
    private readonly IVisitTestResultItemRepository _visitTestResultItemRepository;
    private readonly ISampleTrackingService _sampleTrackingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPatientRepository _patientRepository;

    public EnterTestResultCommandHandler(
        ITestResultRepository testResultRepository,
        IResultValidationService resultValidationService,
        IMedicalHistoryService medicalHistoryService,
        IVisitRepository visitRepository,
        IVisitTestResultItemRepository visitTestResultItemRepository,
        ISampleTrackingService sampleTrackingService,
        IUnitOfWork unitOfWork,
        IPatientRepository patientRepository)
    {
        _testResultRepository = testResultRepository;
        _resultValidationService = resultValidationService;
        _medicalHistoryService = medicalHistoryService;
        _visitRepository = visitRepository;
        _visitTestResultItemRepository = visitTestResultItemRepository;
        _sampleTrackingService = sampleTrackingService;
        _unitOfWork = unitOfWork;
        _patientRepository = patientRepository;
    }

    public async Task<Unit> Handle(EnterTestResultCommand request, CancellationToken cancellationToken)
    {
        var visitTestResultItem = await _visitTestResultItemRepository.GetByIdAsync(request.VisitTestResultItemId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTestResultItem), request.VisitTestResultItemId);

        var visitTest = await _visitRepository.GetVisitTestAsync(visitTestResultItem.VisitTestId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTest), visitTestResultItem.VisitTestId);

        var patient = await _patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Patient), request.PatientId);

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

        var gender = patient.Gender == Gender.Male ? "male" : "female";
        var patientAge = new Age(request.AgeYears, request.AgeMonths, request.AgeDays, request.AgeUnit);
        var isPregnant = patient.Pregnancy;

        var validationResult = await _resultValidationService.ValidateResultAsync(
            request.VisitTestResultItemId,
            request.Value,
            gender,
            patientAge,
            isPregnant,
            cancellationToken);

        var testResult = TestResult.Enter(request.VisitTestResultItemId, request.Value, request.EnteredByUserId);

        testResult.Unit = request.Unit;
        testResult.ReferenceRange = validationResult.ReferenceRange ?? string.Empty;
        testResult.Status = validationResult.Status;
        testResult.OverrideReason = request.OverrideReason;

        if (validationResult.WarningComment != null)
        {
            testResult.SetComment(validationResult.WarningComment);
        }

        await _testResultRepository.AddAsync(testResult, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _medicalHistoryService.ShouldAutoInsertHistoryAsync(request.PatientId, visitTestResultItem.VisitTestId, cancellationToken);

        return Unit.Value;
    }
}
