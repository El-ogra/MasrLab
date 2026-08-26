namespace MasrLab.Domain.Services;

public readonly record struct DerivedResultInput(string ComponentName, string Value);

// OQ-M4-6 registry kinds — Concentration slot is reserved for future configuration
// (formula must be supplied by project owner; DO NOT invent a clinical calculation).
public enum DerivedFormulaKind
{
    Inr = 1,
    Ratio = 2,
    // OQ-M4-6 — Concentration formula kind is reserved/inactive until the project
    // owner provides an explicit, documented formula. The kind exists in the
    // registry structure but is intentionally unregistered from IsDerivedTarget/TryCompute.
    Concentration = 3
}

public readonly record struct DerivedFormulaDescriptor(DerivedFormulaKind Kind, string TargetComponentName);

// OQ-M4-6: derived analytes (INR, ratios, ...) self-compute from sibling result items
// within one VisitTest. Missing inputs and divide-by-zero leave the slot blank.
public interface IDerivedResultCalculator
{
    bool IsDerivedTarget(string componentName);

    bool TryCompute(string componentName, IReadOnlyList<DerivedResultInput> siblingValues, out string computedValue);
}
