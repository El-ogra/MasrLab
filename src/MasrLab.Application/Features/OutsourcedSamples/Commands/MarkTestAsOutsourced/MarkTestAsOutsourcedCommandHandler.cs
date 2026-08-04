using MediatR;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.MarkTestAsOutsourced;

public class MarkTestAsOutsourcedCommandHandler : IRequestHandler<MarkTestAsOutsourcedCommand, Unit>
{
    private readonly IRepository<OutsourcedSample> _outsourcedRepository;
    private readonly IRepository<VisitTest> _visitTestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkTestAsOutsourcedCommandHandler(
        IRepository<OutsourcedSample> outsourcedRepository,
        IRepository<VisitTest> visitTestRepository,
        IUnitOfWork unitOfWork)
    {
        _outsourcedRepository = outsourcedRepository;
        _visitTestRepository = visitTestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(MarkTestAsOutsourcedCommand request, CancellationToken cancellationToken)
    {
        var visitTest = await _visitTestRepository.GetByIdAsync(request.TestId);
        if (visitTest is null)
            throw new InvalidOperationException($"VisitTest with Id {request.TestId} not found.");

        var outsourcedSample = new OutsourcedSample
        {
            PatientVisitId = request.PatientVisitId,
            TestId = request.TestId
        };

        outsourcedSample.Send(request.ExternalLabId, request.CostPrice);

        await _outsourcedRepository.AddAsync(outsourcedSample);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
