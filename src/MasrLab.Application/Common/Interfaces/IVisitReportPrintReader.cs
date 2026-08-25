namespace MasrLab.Application.Common.Interfaces;

using MasrLab.Application.Common.Printing;

public sealed record VisitReportPrintData(
    ClinicalReportPrintDto Payload,
    DateTime? LastPrintedAtUtc,
    int? LastPrintedByUserId,
    IReadOnlyList<int> EnteredTestResultIds,
    int? CultureVisitTestResultItemId);

// Slice 11: builds the print payload for a visit report without mutating anything.
public interface IVisitReportPrintReader
{
    Task<VisitReportPrintData?> GetAsync(
        int visitTestId,
        VisitReportKind kind,
        int? reportId,
        CancellationToken cancellationToken = default);
}
