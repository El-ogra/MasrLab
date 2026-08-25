using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.SaveConsolidatedReport;

// M4-BR-10/11: persists the composition with user ordering.
public sealed record SaveConsolidatedReportCommand(
    int PatientVisitId,
    IReadOnlyList<int> VisitTestIds,
    bool PrintGroupSubtitles = true,
    string? Comment = null) : IRequest<int>;

public class SaveConsolidatedReportCommandHandler : IRequestHandler<SaveConsolidatedReportCommand, int>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IRepository<ConsolidatedReport> _reportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveConsolidatedReportCommandHandler(
        IVisitRepository visitRepository,
        IRepository<ConsolidatedReport> reportRepository,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(SaveConsolidatedReportCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        if (!visit.VisitTests.Any())
            throw new BusinessRuleViolationException("Cannot compose a consolidated report for a visit with no tests.");

        var ownedIds = visit.VisitTests.Select(vt => vt.Id).ToHashSet();

        var report = ConsolidatedReport.Create(request.PatientVisitId, request.PrintGroupSubtitles);
        report.SetComment(request.Comment);

        foreach (var visitTestId in request.VisitTestIds)
        {
            if (!ownedIds.Contains(visitTestId))
                throw new BusinessRuleViolationException(
                    $"Visit test {visitTestId} does not belong to visit {request.PatientVisitId}.");
            // OQ-M4-14: un-entered tests are admitted — composition does not require results.
            report.AddItem(visitTestId);
        }

        await _reportRepository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return report.Id;
    }
}
