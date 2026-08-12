using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

public class TestsMasterDataUnitAndTurnaroundValidationTests
{
    private static AddTestCommand CreateAddCommand(string? turnaroundTime = null, string? unit = null) =>
        new("CBC", "CBC Report", "CBC Receipt", "Hematology", "B1", 120m,
            turnaroundTime ?? "24h", false, unit ?? "mg", null, null, null, null, null, null,
            false, false, false, false, 1, 0, ReferenceType.General, null, null, null, null, null,
            false, null, null, null);

    private static UpdateTestCommand CreateUpdateCommand(string? turnaroundTime = null, string? unit = null) =>
        new(1, "CRP", "CRP Report", "CRP Receipt", "Chemistry", null, 220m,
            turnaroundTime ?? "48h", true, unit ?? "mg/L", null, null, null, null, null, null,
            false, false, false, false, 2, 1, ReferenceType.General, null, null, null, null, null,
            false, null, null, null);

    [Fact]
    public void AddTest_validator_rejects_empty_TurnaroundTime()
    {
        var result = new AddTestCommandValidator().Validate(CreateAddCommand(turnaroundTime: string.Empty));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddTestCommand.TurnaroundTime));
    }

    [Fact]
    public void AddTest_validator_rejects_empty_Unit()
    {
        var result = new AddTestCommandValidator().Validate(CreateAddCommand(unit: string.Empty));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddTestCommand.Unit));
    }

    [Fact]
    public void UpdateTest_validator_rejects_empty_TurnaroundTime()
    {
        var result = new UpdateTestCommandValidator().Validate(CreateUpdateCommand(turnaroundTime: string.Empty));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTestCommand.TurnaroundTime));
    }

    [Fact]
    public void UpdateTest_validator_rejects_empty_Unit()
    {
        var result = new UpdateTestCommandValidator().Validate(CreateUpdateCommand(unit: string.Empty));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTestCommand.Unit));
    }

    [Fact]
    public void AddTest_validator_accepts_non_empty_TurnaroundTime_and_Unit()
    {
        var result = new AddTestCommandValidator().Validate(CreateAddCommand());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateTest_validator_accepts_non_empty_TurnaroundTime_and_Unit()
    {
        var result = new UpdateTestCommandValidator().Validate(CreateUpdateCommand());
        Assert.True(result.IsValid);
    }
}
