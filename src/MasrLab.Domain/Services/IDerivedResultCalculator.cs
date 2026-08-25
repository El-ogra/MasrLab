namespace MasrLab.Domain.Services;

public readonly record struct DerivedResultInput(string ComponentName, string Value);

// OQ-M4-6: derived analytes (INR, ratios, ...) self-compute from sibling result items
// within one VisitTest. Missing inputs and divide-by-zero leave the slot blank.
public interface IDerivedResultCalculator
{
    bool IsDerivedTarget(string componentName);

    bool TryCompute(string componentName, IReadOnlyList<DerivedResultInput> siblingValues, out string computedValue);
}
