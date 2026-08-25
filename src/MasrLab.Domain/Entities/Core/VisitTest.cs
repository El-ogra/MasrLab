using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class VisitTest : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }

    private decimal _price;
    public decimal Price
    {
        get => _price;
        internal set
        {
            if (value < 0)
                throw new BusinessRuleViolationException("Price cannot be negative.");
            _price = value;
        }
    }

    public bool IsOutsourced { get; private set; }
    public int? ReceiptId { get; set; }

    public string TestNameSnapshot { get; set; } = string.Empty;
    public string ReportNameSnapshot { get; set; } = string.Empty;
    public string ReceiptNameSnapshot { get; set; } = string.Empty;
    public bool IsCompoundSnapshot { get; set; }
    public int? VisitCommercialPackageId { get; set; }

    public int? SourceTestGroupId { get; set; }
    public string? TestGroupNameSnapshot { get; set; }

    // Workflow flags (M4-BR-05): Finish → Verify → Print (OQ-M4-2); Export is a stored
    // NO-OP placeholder marker (OQ-M4-3) reserved for a future local PDF/Excel generator.
    public bool IsFinished { get; private set; }
    public int? FinishedByUserId { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public bool IsVerified { get; private set; }
    public int? VerifiedByUserId { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public bool IsPrinted { get; private set; }
    public int? PrintedByUserId { get; private set; }
    public DateTime? PrintedAt { get; private set; }
    public bool IsExportMarked { get; private set; }

    public void MarkFinished(int userId)
    {
        if (IsFinished)
            return;
        if (userId <= 0)
            throw new BusinessRuleViolationException("A valid user is required to finish a test.");
        IsFinished = true;
        FinishedByUserId = userId;
        FinishedAt = DateTime.UtcNow;
    }

    public void MarkVerified(int userId)
    {
        if (IsVerified)
            return;
        if (!IsFinished)
            throw new BusinessRuleViolationException("A test must be finished before it can be verified.");
        if (userId <= 0)
            throw new BusinessRuleViolationException("A valid user is required to verify a test.");
        IsVerified = true;
        VerifiedByUserId = userId;
        VerifiedAt = DateTime.UtcNow;
    }

    // OQ-M4-2 binding rule: printing is physically impossible before verification.
    public void MarkPrinted(int userId)
    {
        if (IsPrinted)
            return;
        if (!IsVerified)
            throw new BusinessRuleViolationException("A test must be verified before it can be printed.");
        if (userId <= 0)
            throw new BusinessRuleViolationException("A valid user is required to print a test.");
        IsPrinted = true;
        PrintedByUserId = userId;
        PrintedAt = DateTime.UtcNow;
    }

    // OQ-M4-3: stored marker only — no events, no transmission of any kind.
    public void SetExportMark(bool marked)
    {
        IsExportMarked = marked;
    }

    public ICollection<VisitTestResultItem> ResultItems { get; set; } = new List<VisitTestResultItem>();

    public VisitTest(int patientVisitId, int testId, decimal price, bool isOutsourced)
    {
        PatientVisitId = patientVisitId;
        TestId = testId;
        Price = price;
        IsOutsourced = isOutsourced;
    }
}
