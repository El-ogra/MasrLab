using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.ReorderConsolidatedReportItem;

// Up/down reorder of one member inside the persisted composition (M4-BR-11).
public sealed record ReorderConsolidatedReportItemCommand(
    int ConsolidatedReportId,
    int VisitTestId,
    bool MoveUp) : IRequest;

public class ReorderConsolidatedReportItemCommandHandler : IRequestHandler<ReorderConsolidatedReportItemCommand>
{
    private readonly IRepository<ConsolidatedReport> _reportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReorderConsolidatedReportItemCommandHandler(
        IRepository<ConsolidatedReport> reportRepository,
        IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReorderConsolidatedReportItemCommand request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(request.ConsolidatedReportId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(ConsolidatedReport), request.ConsolidatedReportId);

        if (request.MoveUp)
            report.MoveUp(request.VisitTestId);
        else
            report.MoveDown(request.VisitTestId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
