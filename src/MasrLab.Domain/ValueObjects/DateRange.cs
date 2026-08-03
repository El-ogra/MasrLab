namespace MasrLab.Domain.ValueObjects;

public record DateRange
{
    public DateTime Start { get; }
    public DateTime? End { get; }

    public DateRange(DateTime start, DateTime? end)
    {
        if (end.HasValue && end.Value < start)
            throw new ArgumentException("End date must be greater than or equal to Start date", nameof(end));

        Start = start;
        End = end;
    }

    public bool Contains(DateTime date) => date >= Start && (!End.HasValue || date <= End.Value);

    public TimeSpan Duration => (End ?? DateTime.UtcNow) - Start;
}
