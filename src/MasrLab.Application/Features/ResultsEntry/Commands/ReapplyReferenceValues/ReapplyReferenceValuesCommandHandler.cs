using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.ResultsEntry.Commands.ReapplyReferenceValues;

public sealed class ReapplyReferenceValuesCommandHandler
    : IRequestHandler<ReapplyReferenceValuesCommand, Unit>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IVisitTestResultItemRepository _visitTestResultItemRepository;
    private readonly IVisitRepository _visitRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IResultValidationService _resultValidationService;
    private readonly IReferenceValueRepository _referenceValueRepository;
    private readonly IReferenceValueMatcher _referenceValueMatcher;
    private readonly IRepository<TestResultEditHistory> _historyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReapplyReferenceValuesCommandHandler(
        ITestResultRepository testResultRepository,
        IVisitTestResultItemRepository visitTestResultItemRepository,
        IVisitRepository visitRepository,
        IPatientRepository patientRepository,
        IResultValidationService resultValidationService,
        IReferenceValueRepository referenceValueRepository,
        IReferenceValueMatcher referenceValueMatcher,
        IRepository<TestResultEditHistory> historyRepository,
        IUnitOfWork unitOfWork)
    {
        _testResultRepository = testResultRepository;
        _visitTestResultItemRepository = visitTestResultItemRepository;
        _visitRepository = visitRepository;
        _patientRepository = patientRepository;
        _resultValidationService = resultValidationService;
        _referenceValueRepository = referenceValueRepository;
        _referenceValueMatcher = referenceValueMatcher;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        ReapplyReferenceValuesCommand request,
        CancellationToken cancellationToken)
    {
        var testResult = await _testResultRepository.GetByIdAsync(
            request.TestResultId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestResult), request.TestResultId);

        var resultItem = await _visitTestResultItemRepository.GetByIdAsync(
            testResult.VisitTestResultItemId, cancellationToken)
            ?? throw new EntityNotFoundException(
                nameof(VisitTestResultItem), testResult.VisitTestResultItemId);

        var visitTest = await _visitRepository.GetVisitTestAsync(
            resultItem.VisitTestId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTest), resultItem.VisitTestId);

        var visit = await _visitRepository.GetByIdAsync(
            visitTest.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), visitTest.PatientVisitId);

        if (visit.Status != VisitStatus.ResultsEntered)
            throw new BusinessRuleViolationException(
                "Visit must be in ResultsEntered status to reapply reference values.");

        var patient = await _patientRepository.GetByIdAsync(
            visit.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Patient), visit.PatientId);

        var gender = patient.Gender == Gender.Male ? "male" : "female";
        var oldComment = testResult.Comment;
        var previousAutoComment = await ReconstructPreviousAutoCommentAsync(
            testResult,
            resultItem.SourceTestComponentId,
            gender,
            patient.Age,
            patient.Pregnancy,
            cancellationToken);

        var validationResult = await _resultValidationService.ValidateResultAsync(
            testResult.VisitTestResultItemId,
            testResult.Value,
            gender,
            patient.Age,
            patient.Pregnancy,
            cancellationToken);

        testResult.ReapplyReference(
            validationResult.ReferenceRange ?? string.Empty,
            validationResult.Status,
            validationResult.WarningComment,
            previousAutoComment,
            request.AppliedByUserId);

        if (testResult.PrintCount > 0)
            testResult.MarkReprintRequired();

        var commentChanged = !string.Equals(
            oldComment, testResult.Comment, StringComparison.Ordinal);
        var history = new TestResultEditHistory
        {
            TestResultId = testResult.Id,
            OldValue = null,
            NewValue = null,
            OldComment = commentChanged ? oldComment : null,
            NewComment = commentChanged ? testResult.Comment : null,
            ChangeType = ResultEditChangeType.ReferenceReapplied,
            EditedByUserId = request.AppliedByUserId,
            EditedAt = DateTime.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task<string?> ReconstructPreviousAutoCommentAsync(
        TestResult testResult,
        int testComponentId,
        string gender,
        Age patientAge,
        bool isPregnant,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(testResult.ReferenceRange))
            return null;

        var candidates = await _referenceValueRepository.GetByTestComponentIdAsync(
            testComponentId, cancellationToken);
        var previousRangeCandidates = candidates
            .Where(value => string.Equals(
                value.NormalRange,
                testResult.ReferenceRange,
                StringComparison.Ordinal))
            .ToList();

        if (previousRangeCandidates.Count == 0)
            return null;

        var previousMatch = _referenceValueMatcher.Match(
            previousRangeCandidates,
            testComponentId,
            gender,
            patientAge,
            isPregnant);

        if (previousMatch.Kind != ReferenceMatchKind.Matched)
            return null;

        return testResult.Status switch
        {
            ResultStatus.High => previousMatch.MatchedValue?.HighComment,
            ResultStatus.Low => previousMatch.MatchedValue?.LowComment,
            _ => null
        };
    }
}
