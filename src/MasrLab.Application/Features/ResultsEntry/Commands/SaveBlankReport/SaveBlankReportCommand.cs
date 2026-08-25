using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.SaveBlankReport;

// OQ-M4-8: persists a blank, re-printable report against the patient visit.
public sealed record SaveBlankReportCommand(
    int PatientVisitId,
    string? ReportTitle = null,
    string? Comment = null,
    string? PaginationNote = null) : IRequest<int>;

public class SaveBlankReportCommandHandler : IRequestHandler<SaveBlankReportCommand, int>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IRepository<Domain.Entities.Core.BlankReport> _blankReportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveBlankReportCommandHandler(
        IVisitRepository visitRepository,
        IRepository<Domain.Entities.Core.BlankReport> blankReportRepository,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _blankReportRepository = blankReportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(SaveBlankReportCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        if (!visit.VisitTests.Any())
            throw new BusinessRuleViolationException("Cannot create a blank report for a visit with no tests.");

        var report = Domain.Entities.Core.BlankReport.Create(
            request.PatientVisitId,
            request.ReportTitle ?? "تقرير فارغ",
            request.Comment,
            request.PaginationNote ?? "يتبع في الصفحة التالية");

        foreach (var visitTest in visit.VisitTests.OrderBy(vt => vt.Id))
        {
            report.AddRow(
                ResolveRowTitle(visitTest),
                string.Empty,   // blank form — filled in by hand.
                string.Empty,
                string.Empty,
                string.Empty);
        }

        await _blankReportRepository.AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return report.Id;
    }

    private static string ResolveRowTitle(VisitTest visitTest) =>
        !string.IsNullOrWhiteSpace(visitTest.ReceiptNameSnapshot)
            ? visitTest.ReceiptNameSnapshot
            : !string.IsNullOrWhiteSpace(visitTest.TestNameSnapshot)
                ? visitTest.TestNameSnapshot
                : $"Test #{visitTest.TestId}";
}
