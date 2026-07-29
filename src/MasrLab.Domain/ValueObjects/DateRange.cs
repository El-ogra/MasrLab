namespace MasrLab.Domain.ValueObjects;

public record DateRange(DateTime Start, DateTime? End)
{
    public bool Contains(DateTime date) => date >= Start && (!End.HasValue || date <= End.Value);

    public TimeSpan Duration => (End ?? DateTime.UtcNow) - Start;
}
