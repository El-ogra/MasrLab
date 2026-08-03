// TODO: Section 5 of Docs/Test_for_domain.md documents deliberate domain gaps that are
// intentionally NOT covered by tests here (unraised events such as PatientRegistered and
// PatientUpdated, unimplemented invariants such as INV-01/INV-02, and partial state machines).

using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Tests;

public class AgeTests
{
    [Fact]
    public void Constructor_WhenAllComponentsPositive_ShouldCreateInstance()
    {
        var age = new Age(30, 2, 15);

        Assert.Equal(30, age.Years);
        Assert.Equal(2, age.Months);
        Assert.Equal(15, age.Days);
        Assert.Equal(362, age.TotalMonths);
        Assert.Equal("30 years, 2 months, 15 days", age.ToString());
    }

    [Fact]
    public void Constructor_WhenOnlyYearsPositive_ShouldCreateInstance()
    {
        var age = new Age(1, 0, 0);

        Assert.Equal(1, age.Years);
        Assert.Equal(0, age.Months);
        Assert.Equal(0, age.Days);
    }

    [Fact]
    public void Constructor_WhenOnlyDaysPositive_ShouldCreateInstance()
    {
        var age = new Age(0, 0, 1);

        Assert.Equal(0, age.Years);
        Assert.Equal(0, age.Months);
        Assert.Equal(1, age.Days);
    }

    [Fact]
    public void Constructor_WhenYearsNegative_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Age(-1, 0, 0));

        Assert.Contains("Age components cannot be negative", ex.Message);
        Assert.Equal("years", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenMonthsNegative_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Age(0, -1, 0));

        Assert.Contains("Age components cannot be negative", ex.Message);
        Assert.Equal("years", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenDaysNegative_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Age(0, 0, -1));

        Assert.Contains("Age components cannot be negative", ex.Message);
        Assert.Equal("years", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenAllZero_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Age(0, 0, 0));

        Assert.Contains("At least one age component must be greater than zero", ex.Message);
        Assert.Equal("years", ex.ParamName);
    }

    [Fact]
    public void TotalMonths_WhenYearsAndMonthsProvided_ShouldReturnYearsTimes12PlusMonths()
    {
        var age = new Age(2, 3, 0);

        Assert.Equal(27, age.TotalMonths);
    }
}

public class DateRangeTests
{
    [Fact]
    public void Constructor_WhenEndAfterStart_ShouldCreateInstance()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 10));

        Assert.Equal(new DateTime(2026, 1, 1), range.Start);
        Assert.Equal(new DateTime(2026, 1, 10), range.End);
    }

    [Fact]
    public void Constructor_WhenEndEqualsStart_ShouldCreateInstance()
    {
        var start = new DateTime(2026, 1, 1);

        var range = new DateRange(start, start);

        Assert.Equal(start, range.Start);
        Assert.Equal(start, range.End);
    }

    [Fact]
    public void Constructor_WhenEndIsNull_ShouldCreateOpenRange()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), null);

        Assert.Equal(new DateTime(2026, 1, 1), range.Start);
        Assert.Null(range.End);
    }

    [Fact]
    public void Constructor_WhenEndBeforeStart_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new DateRange(new DateTime(2026, 1, 10), new DateTime(2026, 1, 1)));

        Assert.Contains("End date must be greater than or equal to Start date", ex.Message);
        Assert.Equal("end", ex.ParamName);
    }

    [Fact]
    public void Contains_WhenDateWithinRange_ShouldReturnTrue()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

        Assert.True(range.Contains(new DateTime(2026, 1, 15)));
    }

    [Fact]
    public void Contains_WhenDateEqualsStart_ShouldReturnTrue()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

        Assert.True(range.Contains(new DateTime(2026, 1, 1)));
    }

    [Fact]
    public void Contains_WhenDateEqualsEnd_ShouldReturnTrue()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

        Assert.True(range.Contains(new DateTime(2026, 1, 31)));
    }

    [Fact]
    public void Contains_WhenDateBeforeStart_ShouldReturnFalse()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

        Assert.False(range.Contains(new DateTime(2025, 12, 31)));
    }

    [Fact]
    public void Contains_WhenDateAfterEnd_ShouldReturnFalse()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

        Assert.False(range.Contains(new DateTime(2026, 2, 1)));
    }

    [Fact]
    public void Contains_WhenEndIsNullAndDateAfterStart_ShouldReturnTrue()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), null);

        Assert.True(range.Contains(new DateTime(2026, 2, 1)));
    }

    [Fact]
    public void Duration_WhenEndProvided_ShouldReturnEndMinusStart()
    {
        var range = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 11));

        Assert.Equal(TimeSpan.FromDays(10), range.Duration);
    }
}

public class EgyptianPhoneTests
{
    [Fact]
    public void Constructor_WhenValid11Digits_ShouldCreateInstance()
    {
        var phone = new EgyptianPhone("01012345678");

        Assert.Equal("01012345678", phone.Value);
    }

    [Fact]
    public void Constructor_WhenValid10Digits_ShouldCreateInstance()
    {
        var phone = new EgyptianPhone("0112345678");

        Assert.Equal("0112345678", phone.Value);
    }

    [Fact]
    public void Constructor_WhenContainsFormattingButValidDigits_ShouldCreateInstance()
    {
        var phone = new EgyptianPhone("010-1234-5678");

        Assert.Equal("010-1234-5678", phone.Value);
    }

    [Fact]
    public void Constructor_WhenNull_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new EgyptianPhone(null!));

        Assert.Contains("Phone number cannot be empty", ex.Message);
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenEmpty_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new EgyptianPhone(""));

        Assert.Contains("Phone number cannot be empty", ex.Message);
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenWhitespace_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new EgyptianPhone("   "));

        Assert.Contains("Phone number cannot be empty", ex.Message);
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenDigitsLessThan10_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new EgyptianPhone("012345"));

        Assert.Contains("Egyptian phone number must be 10 or 11 digits", ex.Message);
    }

    [Fact]
    public void Constructor_WhenDigitsMoreThan11_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new EgyptianPhone("010123456789"));

        Assert.Contains("Egyptian phone number must be 10 or 11 digits", ex.Message);
    }

    [Fact]
    public void Constructor_WhenValidLengthButDoesNotStartWith01_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new EgyptianPhone("02012345678"));

        Assert.Contains("Egyptian phone number must start with 01", ex.Message);
    }

    [Fact]
    public void Constructor_WhenNonDigitCharactersFillString_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new EgyptianPhone("abcdefghijk"));

        Assert.Contains("Egyptian phone number must be 10 or 11 digits", ex.Message);
    }

    [Fact]
    public void ToString_ShouldReturnOriginalValue()
    {
        var phone = new EgyptianPhone("01012345678");

        Assert.Equal("01012345678", phone.ToString());
    }
}
