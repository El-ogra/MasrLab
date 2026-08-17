using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Services;

public enum ReferenceMatchKind
{
    Matched,
    NoRangeConfigured,
    NoRangeForDemographics
}

public sealed class ReferenceMatchResult
{
    public ReferenceMatchKind Kind { get; }
    public ReferenceValue? MatchedValue { get; }
    public string? MatchedRange { get; }

    private ReferenceMatchResult(ReferenceMatchKind kind, ReferenceValue? matchedValue, string? matchedRange)
    {
        Kind = kind;
        MatchedValue = matchedValue;
        MatchedRange = matchedRange;
    }

    public static ReferenceMatchResult Matched(ReferenceValue value)
        => new(ReferenceMatchKind.Matched, value, value.NormalRange);

    public static ReferenceMatchResult NoRangeConfigured()
        => new(ReferenceMatchKind.NoRangeConfigured, null, null);

    public static ReferenceMatchResult NoRangeForDemographics()
        => new(ReferenceMatchKind.NoRangeForDemographics, null, null);
}
