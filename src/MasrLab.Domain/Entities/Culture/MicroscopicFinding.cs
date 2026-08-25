using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Culture;

// M4-BR-17: one row of the microscopic examination block.
// The Bacteria row is system-derived from the recorded organisms (OQ-M4-11)
// and rejects user edits.
public class MicroscopicFinding : BaseEntity
{
    public int CultureId { get; private set; }
    public MicroscopicFindingRow RowKey { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string ReferenceRange { get; private set; } = string.Empty;

    // OQ-M4-5: unchecked rows are excluded from every rendered report.
    public bool IncludeInPrint { get; set; } = true;

    internal MicroscopicFinding()
    {
    }

    public static MicroscopicFinding Create(
        int cultureId,
        MicroscopicFindingRow rowKey,
        string value,
        string referenceRange = "")
    {
        if (cultureId <= 0)
            throw new BusinessRuleViolationException("A microscopic finding requires a valid culture.");
        if (rowKey == MicroscopicFindingRow.Bacteria)
            throw new BusinessRuleViolationException("The Bacteria row is derived from the recorded organisms and cannot be edited.");
        if (value is null)
            throw new BusinessRuleViolationException("A microscopic finding requires a value.");

        return new MicroscopicFinding
        {
            CultureId = cultureId,
            RowKey = rowKey,
            Value = value.Trim(),
            ReferenceRange = referenceRange?.Trim() ?? string.Empty
        };
    }

    // System-only factory — used for the derived Bacteria row (OQ-M4-11).
    internal static MicroscopicFinding CreateDerived(int cultureId, MicroscopicFindingRow rowKey, string value)
    {
        if (cultureId < 0)
            throw new BusinessRuleViolationException("A microscopic finding requires a valid culture.");

        return new MicroscopicFinding
        {
            CultureId = cultureId,
            RowKey = rowKey,
            Value = value?.Trim() ?? string.Empty,
            ReferenceRange = string.Empty
        };
    }

    // System-only path — used to derive the Bacteria row from recorded organisms.
    internal void SetDerivedValue(string value) => Value = value.Trim();

    public void SetValue(string value)
    {
        if (RowKey == MicroscopicFindingRow.Bacteria)
            throw new BusinessRuleViolationException("The Bacteria row is derived from the recorded organisms and cannot be edited.");
        Value = value?.Trim() ?? string.Empty;
    }
}
