namespace MasrLab.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record PatientRegistered(int PatientId, string FullName) : DomainEvent;

public record PatientUpdated(int PatientId, string[] ChangedFields) : DomainEvent;

public record PatientVisitCreated(int VisitId, int PatientId, int? DoctorId, DateTime VisitDate) : DomainEvent;

public record VisitTestAdded(int VisitId, int TestId, decimal Price, bool IsOutsourced) : DomainEvent;

public record VisitTestRemoved(int VisitId, int TestId) : DomainEvent;

public record SampleCollected(int SampleId, int VisitId, int TestId, int CollectedBy) : DomainEvent;

public record SampleUncollectedReverted(int SampleId, int VisitId) : DomainEvent;

public record TestResultEntered(int ResultId, int VisitTestResultItemId, string Value, int EnteredBy) : DomainEvent;

public record TestResultEdited(int ResultId, int VisitTestResultItemId, string OldValue, string NewValue, int EditedBy) : DomainEvent;

public record ReceiptIssued(int ReceiptId, int VisitId, decimal Total, decimal Paid) : DomainEvent;

public record ReceiptPaymentAdded(int ReceiptId, decimal Amount) : DomainEvent;

public record DiscountApplied(int ReceiptId, decimal DiscountValue) : DomainEvent;

public record OutsourcedSampleSent(int OutsourcedSampleId, int VisitTestId, int ExternalLabId, decimal CostPrice) : DomainEvent;

public record OutsourcedResultReceived(int OutsourcedSampleId, DateTime ReceivedAt) : DomainEvent;

public record CultureRecorded(int CultureId, int VisitTestResultItemId) : DomainEvent;

public record SensitivityRecorded(int SensitivityId, int CultureId) : DomainEvent;

public record CashDeposited(int TransactionId, decimal Amount) : DomainEvent;

public record CashWithdrawn(int TransactionId, decimal Amount) : DomainEvent;

public record CommentAttachedToResult(int CommentId, int TestId) : DomainEvent;

public record VisitClosed(int VisitId, DateTime ClosedAt, decimal FinalTotal) : DomainEvent;
