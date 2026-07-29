using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class VisitTest : BaseEntity
{
    // Price is a snapshot captured at visit time — does not auto-update when the test price list changes.
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public decimal Price { get; internal set; }
    public bool IsOutsourced { get; set; }
}
