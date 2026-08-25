using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.SetPrintInclusion;

// OQ-M4-5: per-row print inclusion for one visit test.
//  - Rows: analyte / culture / microscopic rows (VisitTestResultItem.IncludeInPrint).
//  - CommentBlocks: comment blocks (TestResult.IncludeCommentInPrint).
// Unchecked rows are excluded from every rendered report; readers filter on these flags.
public sealed record SetPrintInclusionCommand(
    int VisitTestId,
    IReadOnlyList<RowPrintInclusion> Rows,
    IReadOnlyList<CommentPrintInclusion>? CommentBlocks = null) : IRequest;

public sealed record RowPrintInclusion(int VisitTestResultItemId, bool Include);

public sealed record CommentPrintInclusion(int TestResultId, bool Include);

public class SetPrintInclusionCommandHandler : IRequestHandler<SetPrintInclusionCommand>
{
    private readonly IVisitRepository _visitRepository;
    private readonly ITestResultRepository _testResultRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPrintInclusionCommandHandler(
        IVisitRepository visitRepository,
        ITestResultRepository testResultRepository,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _testResultRepository = testResultRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetPrintInclusionCommand request, CancellationToken cancellationToken)
    {
        var visitTest = await _visitRepository.GetVisitTestWithResultItemsAsync(request.VisitTestId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTest), request.VisitTestId);

        foreach (var row in request.Rows)
        {
            var item = visitTest.ResultItems.FirstOrDefault(ri => ri.Id == row.VisitTestResultItemId)
                ?? throw new BusinessRuleViolationException(
                    $"Result item {row.VisitTestResultItemId} does not belong to visit test {request.VisitTestId}.");
            item.IncludeInPrint = row.Include;
        }

        if (request.CommentBlocks is not null)
        {
            var ownedItemIds = visitTest.ResultItems.Select(ri => ri.Id).ToHashSet();
            foreach (var block in request.CommentBlocks)
            {
                var result = await _testResultRepository.GetByIdAsync(block.TestResultId, cancellationToken)
                    ?? throw new EntityNotFoundException(nameof(TestResult), block.TestResultId);
                if (!ownedItemIds.Contains(result.VisitTestResultItemId))
                    throw new BusinessRuleViolationException(
                        $"Test result {block.TestResultId} does not belong to visit test {request.VisitTestId}.");
                result.IncludeCommentInPrint = block.Include;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
