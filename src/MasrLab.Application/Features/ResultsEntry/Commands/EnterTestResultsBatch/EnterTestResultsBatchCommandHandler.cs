using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResultsBatch;

public class EnterTestResultsBatchCommandHandler
    : IRequestHandler<EnterTestResultsBatchCommand, Unit>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IResultValidationService _resultValidationService;
    private readonly IVisitCompletionEvaluator _completionEvaluator;
    private readonly IUnitOfWork _unitOfWork;

    public EnterTestResultsBatchCommandHandler(
        IVisitRepository visitRepository,
        IPatientRepository patientRepository,
        IResultValidationService resultValidationService,
        IVisitCompletionEvaluator completionEvaluator,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _patientRepository = patientRepository;
        _resultValidationService = resultValidationService;
        _completionEvaluator = completionEvaluator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(EnterTestResultsBatchCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdWithTestsAndResultItemsAsync(request.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        if (visit.Status != VisitStatus.Registered && visit.Status != VisitStatus.ResultsEntered)
            throw new BusinessRuleViolationException("Visit must be in Registered or ResultsEntered status.");

        var patient = await _patientRepository.GetByIdAsync(visit.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Patient), visit.PatientId);

        var gender = patient.Gender == Gender.Male ? "male" : "female";
        var patientAge = patient.Age;
        var isPregnant = patient.Pregnancy;

        var allResultItemIds = visit.VisitTests
            .SelectMany(vt => vt.ResultItems)
            .ToDictionary(ri => ri.Id, ri => ri);

        foreach (var item in request.Items)
        {
            if (!allResultItemIds.TryGetValue(item.VisitTestResultItemId, out var resultItem))
                throw new BusinessRuleViolationException(
                    $"Result item {item.VisitTestResultItemId} does not belong to visit {request.PatientVisitId}.");

            if (resultItem.ResultEntryKind != ResultEntryKind.Ordinary)
                throw new BusinessRuleViolationException(
                    "Batch entry does not support CultureDetail items.");
        }

        foreach (var item in request.Items)
        {
            var resultItem = allResultItemIds[item.VisitTestResultItemId];

            var validationResult = await _resultValidationService.ValidateResultAsync(
                item.VisitTestResultItemId, item.Value, gender, patientAge, isPregnant, cancellationToken);

            var testResult = TestResult.Enter(item.VisitTestResultItemId, item.Value, request.EnteredByUserId);
            testResult.Unit = item.Unit;
            testResult.ReferenceRange = validationResult.ReferenceRange ?? string.Empty;
            testResult.Status = validationResult.Status;

            if (validationResult.WarningComment != null)
                testResult.SetComment(validationResult.WarningComment);

            ApplyCommentPatch(testResult, item.CommentPatch);
        }

        if (visit.Status == VisitStatus.Registered)
        {
            var allComplete = visit.VisitTests
                .SelectMany(vt => vt.ResultItems)
                .All(ri => request.Items.Any(item => item.VisitTestResultItemId == ri.Id));

            if (allComplete)
                visit.EnterAllResults();
        }

        await _completionEvaluator.EvaluateAsync(visit, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static void ApplyCommentPatch(TestResult testResult, Features.ResultsEntry.Common.CommentPatch? patch)
    {
        if (patch is null || !patch.UpdateComment)
            return;

        testResult.SetComment(patch.NewComment);
    }
}
