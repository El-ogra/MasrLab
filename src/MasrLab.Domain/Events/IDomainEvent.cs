namespace MasrLab.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
