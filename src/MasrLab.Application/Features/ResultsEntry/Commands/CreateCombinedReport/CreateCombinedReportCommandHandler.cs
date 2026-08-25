using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.CreateCombinedReport;

// Stub replaced: the handler now persists a real ConsolidatedReport with user ordering.
// The old EnterAllResults side effect is gone — visit status is untouched.
public class CreateCombinedReportCommandHandler : IRequestHandler<CreateCombinedReportCommand, int>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IRepository<ConsolidatedReport> _reportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCombinedReportCommandHandler(
        IVisitRepository visitRepository,
        IRepository<ConsolidatedReport> reportRepository,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateCombinedReportCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        if (!visit.VisitTests.Any())
            throw new BusinessRuleViolationException("Cannot compose a consolidated report for a visit with no tests.");

        var requestedIds = ParseTestIds(request.TestIds);
        var testsById = visit.VisitTests.ToDictionary(vt => vt.Id);

        var report = ConsolidatedReport.Create(request.PatientVisitId);
        foreach (var visitTestId in requestedIds)
        {
            if (!testsById.ContainsKey(visitTestId))
                throw new BusinessRuleViolationException(
                    $"Visit test {visitTestId} does not belong to visit {request.PatientVisitId}.");
            report.AddItem(visitTestId);
        }

        await _reportRepository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return report.Id;
    }

    public static IReadOnlyList<int> ParseTestIds(string? testIds)
    {
        if (string.IsNullOrWhiteSpace(testIds))
            throw new BusinessRuleViolationException("TestIds cannot be empty.");
        var parsed = new List<int>();
        foreach (var token in testIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(token, out var id) || id <= 0)
                throw new BusinessRuleViolationException($"Invalid visit test id '{token}'.");
            parsed.Add(id);
        }

        return parsed;
    }
}
