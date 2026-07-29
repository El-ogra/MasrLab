namespace MasrLab.Domain.ValueObjects;

public record EgyptianPhone
{
    public string Value { get; }

    public EgyptianPhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty", nameof(value));

        var digits = new string([.. value.Where(char.IsDigit)]);
        if (digits.Length < 10 || digits.Length > 11)
            throw new ArgumentException("Egyptian phone number must be 10 or 11 digits", nameof(value));

        if (!digits.StartsWith("01"))
            throw new ArgumentException("Egyptian phone number must start with 01", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;
}
