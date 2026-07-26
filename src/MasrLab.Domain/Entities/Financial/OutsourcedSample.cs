using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Financial;

public class OutsourcedSample : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public int ExternalLabId { get; set; }
    public decimal CostPrice { get; set; }
    public string SettlementStatus { get; set; } = string.Empty;
}
