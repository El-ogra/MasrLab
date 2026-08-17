using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Culture;

public class CulturePrintReceipt : BaseEntity
{
    public int VisitTestResultItemId { get; set; }
    public int PrintedByUserId { get; set; }
    public DateTime PrintedAt { get; set; }
    public int PrintCount { get; set; }
}
