namespace MasrLab.Domain.ValueObjects;

public record Age
{
    public int Years { get; }
    public int Months { get; }
    public int Days { get; }

    public Age(int years, int months, int days)
    {
        if (years < 0 || months < 0 || days < 0)
            throw new ArgumentException("Age components cannot be negative", nameof(years));
        if (years == 0 && months == 0 && days == 0)
            throw new ArgumentException("At least one age component must be greater than zero", nameof(years));

        Years = years;
        Months = months;
        Days = days;
    }

    public int TotalMonths => Years * 12 + Months;

    public override string ToString() => $"{Years} years, {Months} months, {Days} days";
}
