using MediatR;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateCombinedReport;

public class CreateCombinedReportCommandHandler : IRequestHandler<CreateCombinedReportCommand, Unit>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCombinedReportCommandHandler(IVisitRepository visitRepository, IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreateCombinedReportCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdAsync(request.PatientVisitId)
            ?? throw new Exception($"Patient visit with ID {request.PatientVisitId} not found.");

        visit.EnterAllResults();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
