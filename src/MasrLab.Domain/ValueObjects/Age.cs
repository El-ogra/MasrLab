namespace MasrLab.Domain.ValueObjects;

public record Age(int Years, int Months, int Days)
{
    public int TotalMonths => Years * 12 + Months;

    public override string ToString() => $"{Years} years, {Months} months, {Days} days";
}
