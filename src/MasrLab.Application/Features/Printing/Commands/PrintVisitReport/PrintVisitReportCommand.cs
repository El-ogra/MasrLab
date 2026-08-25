using MediatR;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Application.Features.ResultsEntry.Queries.GetReprintWarning;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Printing.Commands.PrintVisitReport;

public sealed record PrintVisitReportResult(
    bool Printed,
    bool PreviewOnly,
    ReprintWarningDto? ConfirmationRequired,
    ClinicalReportPrintDto? Payload);

// M4-BR-03/04/09 + binding OQ-M4-2/5/7: one command wires Verify gating, reprint
// confirmation, inclusion-flagged payload building, and print-status mutation.
// PreviewOnly renders the identical payload with zero side effects.
public sealed record PrintVisitReportCommand(
    int VisitTestId,
    VisitReportKind Kind,
    int UserId,
    int? ReportId = null,
    bool SuppressReprintWarning = false,
    bool PreviewOnly = false) : IRequest<PrintVisitReportResult>;

public class PrintVisitReportCommandHandler : IRequestHandler<PrintVisitReportCommand, PrintVisitReportResult>
{
    private readonly IRepository<VisitTest> _visitTestRepository;
    private readonly ITestResultRepository _testResultRepository;
    private readonly IRepository<CulturePrintReceipt> _culturePrintReceiptRepository;
    private readonly IRepository<BlankReport> _blankReportRepository;
    private readonly IRepository<ConsolidatedReport> _consolidatedReportRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IVisitReportPrintReader _printReader;
    private readonly IUnitOfWork _unitOfWork;

    public PrintVisitReportCommandHandler(
        IRepository<VisitTest> visitTestRepository,
        ITestResultRepository testResultRepository,
        IRepository<CulturePrintReceipt> culturePrintReceiptRepository,
        IRepository<BlankReport> blankReportRepository,
        IRepository<ConsolidatedReport> consolidatedReportRepository,
        IRepository<User> userRepository,
        IVisitReportPrintReader printReader,
        IUnitOfWork unitOfWork)
    {
        _visitTestRepository = visitTestRepository;
        _testResultRepository = testResultRepository;
        _culturePrintReceiptRepository = culturePrintReceiptRepository;
        _blankReportRepository = blankReportRepository;
        _consolidatedReportRepository = consolidatedReportRepository;
        _userRepository = userRepository;
        _printReader = printReader;
        _unitOfWork = unitOfWork;
    }

    public async Task<PrintVisitReportResult> Handle(PrintVisitReportCommand request, CancellationToken cancellationToken)
    {
        var visitTest = await _visitTestRepository.GetByIdAsync(request.VisitTestId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTest), request.VisitTestId);

        // OQ-M4-2 binding rule: printing is physically impossible before verification.
        if (!visitTest.IsVerified && !request.PreviewOnly)
            throw new BusinessRuleViolationException("A test must be verified before it can be printed.");

        var data = await _printReader.GetAsync(request.VisitTestId, request.Kind, request.ReportId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(VisitTest), request.VisitTestId);

        if (request.PreviewOnly)
            return new PrintVisitReportResult(false, true, null, data.Payload);

        // OQ-M4-7: persisted reprint warning — suppressed by the "without msg." checkbox.
        if (data.LastPrintedAtUtc is not null && !request.SuppressReprintWarning)
        {
            var userName = await ResolveUserNameAsync(data.LastPrintedByUserId, cancellationToken);
            return new PrintVisitReportResult(
                false,
                false,
                new ReprintWarningDto(request.VisitTestId, true, data.LastPrintedAtUtc, userName,
                    ReprintWarningDto.BuildMessage(data.LastPrintedAtUtc.Value, userName ?? $"User {data.LastPrintedByUserId}")),
                null);
        }

        await MarkPrintedAsync(visitTest, data, request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PrintVisitReportResult(true, false, null, data.Payload);
    }

    private async Task MarkPrintedAsync(
        VisitTest visitTest,
        VisitReportPrintData data,
        PrintVisitReportCommand request,
        CancellationToken cancellationToken)
    {
        foreach (var resultId in data.EnteredTestResultIds)
        {
            var result = await _testResultRepository.GetByIdAsync(resultId, cancellationToken);
            if (result is not null)
                result.MarkPrinted(request.UserId); // stamps count + user + time per OQ-M4-7 metadata.
        }

        if (request.Kind == VisitReportKind.Blank && request.ReportId is not null)
        {
            var blankReport = await _blankReportRepository.GetByIdAsync(request.ReportId.Value, cancellationToken);
            blankReport?.MarkPrinted(request.UserId);
        }

        if (request.Kind == VisitReportKind.Consolidated && request.ReportId is not null)
        {
            var consolidatedReport = await _consolidatedReportRepository.GetByIdAsync(request.ReportId.Value, cancellationToken);
            consolidatedReport?.MarkPrinted(request.UserId);
        }

        if (request.Kind == VisitReportKind.Culture && data.CultureVisitTestResultItemId is not null)
        {
            await _culturePrintReceiptRepository.AddAsync(new CulturePrintReceipt
            {
                VisitTestResultItemId = data.CultureVisitTestResultItemId.Value,
                PrintedByUserId = request.UserId,
                PrintedAt = DateTime.UtcNow,
                PrintCount = 1
            }, cancellationToken);
        }

        visitTest.MarkPrinted(request.UserId); // Slice 5 workflow flag — verified state enforced above.
    }

    private async Task<string?> ResolveUserNameAsync(int? userId, CancellationToken cancellationToken)
    {
        if (userId is null)
            return null;
        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        return user?.Username ?? $"User {userId}";
    }
}
