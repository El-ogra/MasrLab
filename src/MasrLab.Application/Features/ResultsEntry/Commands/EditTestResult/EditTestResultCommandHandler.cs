using MediatR;
using MasrLab.Application.Common.Constants;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EditTestResult;

public class EditTestResultCommandHandler
    : IRequestHandler<EditTestResultCommand, Unit>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IVisitTestResultItemRepository _visitTestResultItemRepository;
    private readonly IVisitRepository _visitRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IResultValidationService _resultValidationService;
    private readonly IVisitCompletionEvaluator _completionEvaluator;
    private readonly IRepository<TestResultEditHistory> _historyRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IDerivedResultCalculator _derivedResultCalculator;
    private readonly IUnitOfWork _unitOfWork;

    public EditTestResultCommandHandler(
        ITestResultRepository testResultRepository,
        IVisitTestResultItemRepository visitTestResultItemRepository,
        IVisitRepository visitRepository,
        IPatientRepository patientRepository,
        IResultValidationService resultValidationService,
        IVisitCompletionEvaluator completionEvaluator,
        IRepository<TestResultEditHistory> historyRepository,
        IPermissionRepository permissionRepository,
        IDerivedResultCalculator derivedResultCalculator,
        IUnitOfWork unitOfWork)
    {
        _testResultRepository = testResultRepository;
        _visitTestResultItemRepository = visitTestResultItemRepository;
        _visitRepository = visitRepository;
        _patientRepository = patientRepository;
        _resultValidationService = resultValidationService;
        _completionEvaluator = completionEvaluator;
        _historyRepository = historyRepository;
        _permissionRepository = permissionRepository;
        _derivedResultCalculator = derivedResultCalculator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(EditTestResultCommand request, CancellationToken cancellationToken)
    {
        var testResult = await _testResultRepository.GetByIdAsync(request.TestResultId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestResult), request.TestResultId);

        var resultItem = await _visitTestResultItemRepository.GetByIdAsync(testResult.VisitTestResultItemId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTestResultItem), testResult.VisitTestResultItemId);

        var visitTest = await _visitRepository.GetVisitTestAsync(resultItem.VisitTestId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTest), resultItem.VisitTestId);

        var visit = await _visitRepository.GetByIdAsync(visitTest.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), visitTest.PatientVisitId);

        if (visit.Status != VisitStatus.ResultsEntered)
            throw new BusinessRuleViolationException("Visit must be in ResultsEntered status to edit results.");

        var patient = await _patientRepository.GetByIdAsync(visit.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Patient), visit.PatientId);

        var valueChanged = request.NewValue != null && request.NewValue != testResult.Value;
        var commentChanged = request.CommentPatch is { UpdateComment: true } &&
            !string.Equals(testResult.Comment, request.CommentPatch.NewComment, StringComparison.Ordinal);

        if (!valueChanged && !commentChanged)
            throw new BusinessRuleViolationException("No changes detected.");

        if (testResult.PrintCount > 0)
        {
            if (valueChanged)
            {
                // OQ-M4-15: post-print value edits are allowed ONLY with the ResultEdit
                // capability (Results/EditPrinted); unprivileged edits still throw.
                var grant = await _permissionRepository.GetByUserScreenOperationAsync(
                    request.EditedByUserId, ScreenType.Results, PermissionOperation.EditPrinted, cancellationToken);
                if (grant?.Allowed != true)
                    throw new BusinessRuleViolationException(
                        $"{PermissionNames.ResultEdit} permission is required to edit a printed result.");
            }
        }

        var gender = patient.Gender == Gender.Male ? "male" : "female";
        var patientAge = patient.Age;
        var isPregnant = patient.Pregnancy;

        var oldValue = testResult.Value;
        var oldComment = testResult.Comment;

        if (valueChanged)
        {
            var validationResult = await _resultValidationService.ValidateResultAsync(
                testResult.VisitTestResultItemId, request.NewValue!, gender, patientAge, isPregnant, cancellationToken);

            testResult.Edit(request.NewValue!, request.CommentPatch?.NewComment, request.EditedByUserId);
            testResult.ReferenceRange = validationResult.ReferenceRange ?? string.Empty;
            testResult.Status = validationResult.Status;

            if (request.CommentPatch is not { UpdateComment: true })
            {
                testResult.SetAutomaticComment(validationResult.WarningComment);
            }

            await _completionEvaluator.EvaluateAsync(visit, cancellationToken);
        }
        else
        {
            testResult.EditComment(request.CommentPatch!.NewComment, request.EditedByUserId);
        }

        if (testResult.PrintCount > 0)
            testResult.MarkReprintRequired();

        // OQ-M4-6: overriding an auto-computed derived analyte is flagged in the audit trail.
        var isDerivedOverride = valueChanged && _derivedResultCalculator.IsDerivedTarget(resultItem.ComponentName);

        var history = new TestResultEditHistory
        {
            TestResultId = testResult.Id,
            OldValue = valueChanged ? oldValue : null,
            NewValue = valueChanged ? request.NewValue : null,
            OldComment = commentChanged ? oldComment : null,
            NewComment = commentChanged ? request.CommentPatch?.NewComment : null,
            ChangeType = isDerivedOverride ? ResultEditChangeType.DerivedOverride
                : valueChanged && commentChanged ? ResultEditChangeType.ValueAndComment
                : valueChanged ? ResultEditChangeType.ValueOnly
                : ResultEditChangeType.CommentOnly,
            EditedByUserId = request.EditedByUserId,
            EditedAt = DateTime.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
