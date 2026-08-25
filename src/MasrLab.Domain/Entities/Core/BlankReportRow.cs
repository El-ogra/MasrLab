using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class BlankReportRow : BaseEntity
{
    public int BlankReportId { get; private set; }
    public string TestName { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public string Flag { get; private set; } = string.Empty;
    public string ReferenceRange { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private BlankReportRow()
    {
    }

    internal static BlankReportRow Create(
        int blankReportId,
        string testName,
        string result,
        string unit,
        string flag,
        string referenceRange,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(testName))
            throw new BusinessRuleViolationException("Blank report rows require a test name.");
        if (displayOrder <= 0)
            throw new BusinessRuleViolationException("Display order must be positive.");

        return new BlankReportRow
        {
            BlankReportId = blankReportId,
            TestName = testName.Trim(),
            Result = result?.Trim() ?? string.Empty,
            Unit = unit?.Trim() ?? string.Empty,
            Flag = flag?.Trim() ?? string.Empty,
            ReferenceRange = referenceRange?.Trim() ?? string.Empty,
            DisplayOrder = displayOrder
        };
    }
}
