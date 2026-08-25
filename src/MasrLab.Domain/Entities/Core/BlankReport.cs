using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

// OQ-M4-8: "تقرير فارغ" — a persisted, patient-attached, re-printable blank report.
public class BlankReport : BaseEntity
{
    public int PatientVisitId { get; private set; }
    public string ReportTitle { get; private set; } = string.Empty;
    public string? Comment { get; private set; }

    // Pagination note rendered under the table (manual M4-BR-14/15 layout).
    public string PaginationNote { get; private set; } = string.Empty;

    public DateTime? PrintedAt { get; private set; }
    public int? PrintedByUserId { get; private set; }
    public int PrintCount { get; private set; }

    private readonly List<BlankReportRow> _rows = new();
    public IReadOnlyList<BlankReportRow> Rows => _rows.AsReadOnly();

    private BlankReport()
    {
    }

    public static BlankReport Create(int patientVisitId, string reportTitle, string? comment, string? paginationNote)
    {
        if (patientVisitId <= 0)
            throw new BusinessRuleViolationException("Blank report requires a valid patient visit.");
        if (string.IsNullOrWhiteSpace(reportTitle))
            throw new BusinessRuleViolationException("Blank report requires a title.");

        return new BlankReport
        {
            PatientVisitId = patientVisitId,
            ReportTitle = reportTitle.Trim(),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            PaginationNote = paginationNote?.Trim() ?? string.Empty
        };
    }

    public BlankReportRow AddRow(string testName, string result, string unit, string flag, string referenceRange)
    {
        if (string.IsNullOrWhiteSpace(testName))
            throw new BusinessRuleViolationException("Blank report rows require a test name.");

        var row = BlankReportRow.Create(Id, testName, result, unit, flag, referenceRange, _rows.Count + 1);
        _rows.Add(row);
        return row;
    }

    public void SetComment(string? comment)
    {
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }

    public void MarkPrinted(int printedByUserId)
    {
        PrintCount++;
        PrintedAt = DateTime.UtcNow;
        PrintedByUserId = printedByUserId;
    }
}
