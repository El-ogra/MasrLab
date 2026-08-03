using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Financial;

public class OutsourcedSample : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public int ExternalLabId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal PatientPrice { get; set; }
    public SettlementStatus SettlementStatus { get; private set; }
    public DateTime? ReceivedAt { get; set; }

    public void SetPrices(decimal patientPrice, decimal costPrice)
    {
        if (patientPrice < costPrice)
            throw new BusinessRuleViolationException("Patient price must be greater than or equal to cost price.");
        PatientPrice = patientPrice;
        CostPrice = costPrice;
    }

    public void Send(int externalLabId, decimal costPrice)
    {
        if (SettlementStatus != SettlementStatus.Pending)
            throw new BusinessRuleViolationException("Outsourced sample must be in Pending status to send.");
        ExternalLabId = externalLabId;
        CostPrice = costPrice;
        SettlementStatus = SettlementStatus.Pending;
        AddDomainEvent(new OutsourcedSampleSent(Id, PatientVisitId, externalLabId, costPrice));
    }

    public void ReceiveResult()
    {
        if (ReceivedAt.HasValue)
            throw new BusinessRuleViolationException("Result has already been received for this outsourced sample.");
        ReceivedAt = DateTime.UtcNow;
        SettlementStatus = SettlementStatus.PartiallySettled;
        AddDomainEvent(new OutsourcedResultReceived(Id, ReceivedAt.Value));
    }

    public void CompleteSettlement()
    {
        if (ReceivedAt is null)
            throw new BusinessRuleViolationException("Cannot complete settlement before the result is received.");
        if (SettlementStatus != SettlementStatus.PartiallySettled)
            throw new BusinessRuleViolationException("Outsourced sample must be in PartiallySettled status to complete settlement.");
        SettlementStatus = SettlementStatus.Settled;
    }
}
