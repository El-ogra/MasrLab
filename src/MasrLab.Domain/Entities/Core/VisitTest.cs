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

    public ICollection<VisitTestResultItem> ResultItems { get; set; } = new List<VisitTestResultItem>();

    public VisitTest(int patientVisitId, int testId, decimal price, bool isOutsourced)
    {
        PatientVisitId = patientVisitId;
        TestId = testId;
        Price = price;
        IsOutsourced = isOutsourced;
    }
}
