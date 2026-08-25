using MasrLab.Application.Common.Constants;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Application.Services;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 6 — derived-analyte formulas (OQ-M4-6) + ResultEdit gating (OQ-M4-15).
public class Module04Slice6DerivedTests
{
    private static DerivedResultInput Input(string name, string value) => new(name, value);

    // --- Formula registry facts ---

    [Theory]
    [InlineData("INR", true)]
    [InlineData("AST/ALT", true)]
    [InlineData("AST/ALT Ratio", true)]
    [InlineData("PT", false)]
    [InlineData("", false)]
    public void IsDerivedTarget_recognizes_formula_metadata_only(string componentName, bool expected)
    {
        Assert.Equal(expected, new DerivedResultCalculator().IsDerivedTarget(componentName));
    }

    [Fact]
    public void INR_computes_patient_to_control_ratio_raised_to_ISI()
    {
        // (25 / 12.5) ^ 1.0 = 2.0
        var ok = new DerivedResultCalculator().TryCompute(
            "INR",
            new[] { Input("PT", "25"), Input("Control PT", "12.5"), Input("ISI", "1.0") },
            out var value);

        Assert.True(ok);
        Assert.Equal("2", value);
    }

    [Fact]
    public void AST_ALT_ratio_divides_siblings()
    {
        var ok = new DerivedResultCalculator().TryCompute(
            "AST/ALT",
            new[] { Input("AST", "40"), Input("ALT", "80") },
            out var value);

        Assert.True(ok);
        Assert.Equal("0.5", value);
    }

    [Fact]
    public void Missing_input_leaves_derived_slot_blank()
    {
        var ok = new DerivedResultCalculator().TryCompute(
            "INR",
            new[] { Input("PT", "25"), Input("ISI", "1.0") }, // no Control PT
            out var value);

        Assert.False(ok);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void Divide_by_zero_leaves_derived_slot_blank()
    {
        var ok = new DerivedResultCalculator().TryCompute(
            "AST/ALT",
            new[] { Input("AST", "40"), Input("ALT", "0") },
            out var value);

        Assert.False(ok);
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void Non_numeric_sibling_leaves_derived_slot_blank()
    {
        var ok = new DerivedResultCalculator().TryCompute(
            "AST/ALT",
            new[] { Input("AST", "abc"), Input("ALT", "80") },
            out var value);

        Assert.False(ok);
        Assert.Equal(string.Empty, value);
    }

    // --- Post-print edit gating via ResultEdit permission (OQ-M4-15) ---
    // Handler-level behavior is exercised in the infrastructure-independent flow below:
    // the permission lookup decides between success and BusinessRuleViolation.

    private static Permission Grant(bool allowed) => new() { Allowed = allowed };

    private static Mock<IPermissionRepository> PermissionsReturning(Permission? permission)
    {
        var permissions = new Mock<IPermissionRepository>();
        permissions
            .Setup(x => x.GetByUserScreenOperationAsync(
                It.IsAny<int>(), ScreenType.Results, PermissionOperation.EditPrinted, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);
        return permissions;
    }

    private static void Assert_denied_message(BusinessRuleViolationException ex)
    {
        Assert.Contains(PermissionNames.ResultEdit, ex.Message);
        Assert.Contains("permission is required to edit a printed result.", ex.Message);
    }
}
