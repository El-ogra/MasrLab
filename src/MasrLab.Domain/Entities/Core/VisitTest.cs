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

    public bool IsOutsourced { get; set; }

    public TestResult? TestResult { get; set; }
}
