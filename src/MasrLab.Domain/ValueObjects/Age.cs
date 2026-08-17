using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.ValueObjects;

public record Age
{
    public int Years { get; }
    public int Months { get; }
    public int Days { get; }
    public AgeUnit RecordedUnit { get; }

    public Age(int years, int months, int days)
    {
        if (years < 0 || months < 0 || days < 0)
            throw new ArgumentException("Age components cannot be negative", nameof(years));
        if (years == 0 && months == 0 && days == 0)
            throw new ArgumentException("At least one age component must be greater than zero", nameof(years));

        Years = years;
        Months = months;
        Days = days;
        RecordedUnit = DeriveRecordedUnit(years, months, days);
    }

    public Age(int years, int months, int days, AgeUnit recordedUnit)
        : this(years, months, days)
    {
        if (!Enum.IsDefined(recordedUnit))
            throw new ArgumentOutOfRangeException(nameof(recordedUnit), recordedUnit, "Unsupported age unit.");

        RecordedUnit = recordedUnit;
    }

    public Age(int value, AgeUnit unit)
    {
        if (value <= 0)
            throw new ArgumentException("Age value must be greater than zero", nameof(value));

        switch (unit)
        {
            case AgeUnit.Years:
                Years = value;
                Months = 0;
                Days = 0;
                break;
            case AgeUnit.Months:
                Years = 0;
                Months = value;
                Days = 0;
                break;
            case AgeUnit.Days:
                Years = 0;
                Months = 0;
                Days = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unsupported age unit.");
        }

        RecordedUnit = unit;
    }

    public int TotalMonths => Years * 12 + Months;

    private static AgeUnit DeriveRecordedUnit(int years, int months, int days)
    {
        if (years == 0 && months == 0)
            return AgeUnit.Days;

        if (years == 0 && months > 0)
            return AgeUnit.Months;

        return AgeUnit.Years;
    }

    public override string ToString() => $"{Years} years, {Months} months, {Days} days";
}
