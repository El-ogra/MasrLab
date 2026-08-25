using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.SetVisitTestWorkflowFlags;

// Single command toggling the Finish/Verify/Print/Export workflow columns (M4-BR-05).
// Transitions are applied in order; the domain enforces Finish → Verify → Print (OQ-M4-2).
public sealed record SetVisitTestWorkflowFlagsCommand(
    int VisitTestId,
    bool Finish,
    bool Verify,
    bool Print,
    bool Export,
    int UserId
) : IRequest;

public class SetVisitTestWorkflowFlagsCommandHandler : IRequestHandler<SetVisitTestWorkflowFlagsCommand>
{
    private readonly IRepository<VisitTest> _visitTestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetVisitTestWorkflowFlagsCommandHandler(IRepository<VisitTest> visitTestRepository, IUnitOfWork unitOfWork)
    {
        _visitTestRepository = visitTestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetVisitTestWorkflowFlagsCommand request, CancellationToken cancellationToken)
    {
        var visitTest = await _visitTestRepository.GetByIdAsync(request.VisitTestId, cancellationToken);
        if (visitTest is null)
            throw new EntityNotFoundException(nameof(VisitTest), request.VisitTestId);

        if (request.Finish)
            visitTest.MarkFinished(request.UserId);
        if (request.Verify)
            visitTest.MarkVerified(request.UserId);
        if (request.Print)
            visitTest.MarkPrinted(request.UserId);
        visitTest.SetExportMark(request.Export); // OQ-M4-3: no side effects.

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
