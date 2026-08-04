using MediatR;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateBlankReport;

public class CreateBlankReportCommandHandler : IRequestHandler<CreateBlankReportCommand, Unit>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBlankReportCommandHandler(IVisitRepository visitRepository, IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreateBlankReportCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdAsync(request.PatientVisitId)
            ?? throw new Exception($"Patient visit with ID {request.PatientVisitId} not found.");

        visit.IssueReceipt();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
