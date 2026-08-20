using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.ApplyCommentTemplate;

public class ApplyCommentTemplateCommandHandler : IRequestHandler<ApplyCommentTemplateCommand, Unit>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IVisitTestResultItemRepository _visitTestResultItemRepository;
    private readonly IVisitRepository _visitRepository;
    private readonly IRepository<CommentTemplate> _commentTemplateRepository;
    private readonly IRepository<TestResultEditHistory> _historyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyCommentTemplateCommandHandler(
        ITestResultRepository testResultRepository,
        IVisitTestResultItemRepository visitTestResultItemRepository,
        IVisitRepository visitRepository,
        IRepository<CommentTemplate> commentTemplateRepository,
        IRepository<TestResultEditHistory> historyRepository,
        IUnitOfWork unitOfWork)
    {
        _testResultRepository = testResultRepository;
        _visitTestResultItemRepository = visitTestResultItemRepository;
        _visitRepository = visitRepository;
        _commentTemplateRepository = commentTemplateRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ApplyCommentTemplateCommand request, CancellationToken cancellationToken)
    {
        var testResult = await _testResultRepository.GetByIdAsync(request.TestResultId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestResult), request.TestResultId);

        var resultItem = await _visitTestResultItemRepository.GetByIdAsync(
            testResult.VisitTestResultItemId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTestResultItem), testResult.VisitTestResultItemId);

        var visitTest = await _visitRepository.GetVisitTestAsync(resultItem.VisitTestId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTest), resultItem.VisitTestId);

        var visit = await _visitRepository.GetByIdAsync(
            visitTest.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), visitTest.PatientVisitId);

        if (visit.Status != VisitStatus.ResultsEntered && visit.Status != VisitStatus.Printed)
            throw new BusinessRuleViolationException(
                "Visit must be in ResultsEntered or Printed status to apply a comment template.");

        var template = await _commentTemplateRepository.GetByIdAsync(
            request.CommentTemplateId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CommentTemplate), request.CommentTemplateId);

        if (template.TestId != visitTest.TestId)
            throw new BusinessRuleViolationException(
                "The comment template does not belong to the test result's test.");

        var oldComment = testResult.Comment;
        testResult.EditComment(template.Text, request.AppliedByUserId);

        if (testResult.PrintCount > 0)
            testResult.MarkReprintRequired();

        var history = new TestResultEditHistory
        {
            TestResultId = testResult.Id,
            OldValue = null,
            NewValue = null,
            OldComment = oldComment,
            NewComment = template.Text,
            ChangeType = ResultEditChangeType.CommentOnly,
            EditedByUserId = request.AppliedByUserId,
            EditedAt = DateTime.UtcNow
        };

        await _historyRepository.AddAsync(history, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
