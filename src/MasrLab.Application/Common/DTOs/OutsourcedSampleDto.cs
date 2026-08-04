using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record OutsourcedSampleDto
{
    public int Id { get; init; }
    public int PatientVisitId { get; init; }
    public int TestId { get; init; }
    public int ExternalLabId { get; init; }
    public decimal CostPrice { get; init; }
    public decimal PatientPrice { get; init; }
    public SettlementStatus SettlementStatus { get; init; }
    public DateTime? ReceivedAt { get; init; }
}
