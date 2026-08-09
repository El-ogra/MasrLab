using MasrLab.Application.Features.PatientManagement.Commands.DeliverResults;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
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
        var visit = await _visitRepository.GetByIdAsync(request.PatientVisitId, cancellationToken);
        if (visit is null)
            throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        visit.MarkAsPrinted();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
