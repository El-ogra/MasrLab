using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class VisitTest : BaseEntity
{
    // Price is a snapshot captured at visit time — does not auto-update when the test price list changes.
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

    public VisitTest(int patientVisitId, int testId, decimal price, bool isOutsourced)
    {
        PatientVisitId = patientVisitId;
        TestId = testId;
        Price = price;
        IsOutsourced = isOutsourced;
    }

    public TestResult? TestResult { get; set; }
}
