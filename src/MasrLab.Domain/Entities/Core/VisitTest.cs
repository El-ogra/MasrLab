using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Core;

public class VisitTest : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public decimal Price { get; set; }
    public bool IsOutsourced { get; set; }
}
