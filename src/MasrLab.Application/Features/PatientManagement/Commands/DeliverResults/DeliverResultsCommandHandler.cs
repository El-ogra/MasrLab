using MasrLab.Application.Features.PatientManagement.Commands.DeliverResults;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.DeliverResults;

public class DeliverResultsCommandHandler : IRequestHandler<DeliverResultsCommand, Unit>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeliverResultsCommandHandler(IVisitRepository visitRepository, IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeliverResultsCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdAsync(request.PatientVisitId);
        if (visit is null)
            throw new InvalidOperationException($"PatientVisit with Id {request.PatientVisitId} not found.");

        visit.MarkAsPrinted();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
