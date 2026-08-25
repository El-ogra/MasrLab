using System.Globalization;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// OQ-M4-6: registry of derived-analyte formulas keyed off M10 component metadata
/// (component names), never off test names. Operating scope: the sibling result
/// items of a single VisitTest.
///
/// Supported targets:
///   "INR"        = (PatientPT / ControlPT) ^ ISI          — siblings: PT, Control PT, ISI
///   "X/Y"        = X / Y                                   — e.g. "AST/ALT", "A/G"
///   "X/Y RATIO"  = X / Y                                   — e.g. "AST/ALT Ratio"
///   "CONCENTRATION (OF Y IN X)" style targets are covered by the generic ratio rule.
/// </summary>
public sealed class DerivedResultCalculator : IDerivedResultCalculator
{
    private const string InrTarget = "INR";

    public bool IsDerivedTarget(string componentName)
    {
        var normalized = Normalize(componentName);
        if (normalized.Length == 0)
            return false;
        if (normalized == InrTarget)
            return true;
        return TrySplitRatio(normalized, out _, out _);
    }

    public bool TryCompute(string componentName, IReadOnlyList<DerivedResultInput> siblingValues, out string computedValue)
    {
        computedValue = string.Empty;
        var normalized = Normalize(componentName);
        if (normalized.Length == 0 || siblingValues.Count == 0)
            return false;

        if (normalized == InrTarget)
            return TryComputeInr(siblingValues, out computedValue);

        if (TrySplitRatio(normalized, out var numeratorName, out var denominatorName))
            return TryComputeRatio(numeratorName, denominatorName, siblingValues, out computedValue);

        return false;
    }

    private static bool TryComputeInr(IReadOnlyList<DerivedResultInput> siblings, out string computedValue)
    {
        computedValue = string.Empty;
        var patientPt = FindDecimal(siblings, "PT");
        var controlPt = FindDecimal(siblings, "CONTROL PT", "CONTROLPT", "CONTROLP");
        var isi = FindDecimal(siblings, "ISI");

        if (patientPt is not { } pt || controlPt is not { } control || isi is not { } sensitivity
            || control <= 0 || sensitivity <= 0 || pt <= 0)
            return false; // missing input or degenerate values → leave blank.

        var inr = Math.Pow((double)pt / (double)control, (double)sensitivity);
        computedValue = Math.Round((decimal)inr, 2).ToString(CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryComputeRatio(string numeratorName, string denominatorName, IReadOnlyList<DerivedResultInput> siblings, out string computedValue)
    {
        computedValue = string.Empty;
        var numerator = FindDecimal(siblings, numeratorName);
        var denominator = FindDecimal(siblings, denominatorName);

        if (numerator is not { } n || denominator is not { } d || d == 0)
            return false; // divide-by-zero or missing input → leave blank.

        computedValue = Math.Round(n / d, 2).ToString(CultureInfo.InvariantCulture);
        return true;
    }

    private static decimal? FindDecimal(IReadOnlyList<DerivedResultInput> siblings, params string[] acceptedNames)
    {
        foreach (var input in siblings)
        {
            var name = Normalize(input.ComponentName);
            foreach (var accepted in acceptedNames)
            {
                if (name == Normalize(accepted) && decimal.TryParse(input.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                    return value;
            }
        }

        return null;
    }

    private static bool TrySplitRatio(string normalized, out string numerator, out string denominator)
    {
        numerator = denominator = string.Empty;
        var slashIndex = normalized.IndexOf('/');
        if (slashIndex <= 0 || slashIndex == normalized.Length - 1)
            return false;

        var left = normalized[..slashIndex].Trim();
        var right = normalized[(slashIndex + 1)..].Trim();
        const string ratioSuffix = "RATIO";
        if (right.EndsWith(ratioSuffix, StringComparison.OrdinalIgnoreCase))
            right = right[..^ratioSuffix.Length].Trim();
        if (left.Length == 0 || right.Length == 0 || left == right)
            return false;

        numerator = left;
        denominator = right;
        return true;
    }

    private static string Normalize(string componentName) =>
        componentName?.Trim().Replace(" ", string.Empty).ToUpperInvariant() ?? string.Empty;
}
