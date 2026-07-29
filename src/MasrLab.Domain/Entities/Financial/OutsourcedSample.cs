using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Financial;

public class OutsourcedSample : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public int ExternalLabId { get; set; }
    public decimal CostPrice { get; set; }
    public SettlementStatus SettlementStatus { get; set; }
    public DateTime? ReceivedAt { get; set; }
}
