using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

public class Module12_TestCommandValidatorTests
{
    private readonly AddTestCommandValidator _addValidator = new();
    private readonly UpdateTestCommandValidator _updateValidator = new();

    #region Add — OQ-6: SentOutsideLab validation

    [Fact]
    public void Add_SentOutsideLab_MissingLab_ShouldFail()
    {
        var cmd = CreateAddCommand(sentOutsideLab: true, labId: null, costPrice: 50m);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OutsourcedLabReferralEntityId");
    }

    [Fact]
    public void Add_SentOutsideLab_MissingCostPrice_ShouldFail()
    {
        var cmd = CreateAddCommand(sentOutsideLab: true, labId: 1, costPrice: null);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OutsourcedCostPrice");
    }

    [Fact]
    public void Add_SentOutsideLab_CostPriceZero_ShouldPass()
    {
        var cmd = CreateAddCommand(sentOutsideLab: true, labId: 1, costPrice: 0m, patientPrice: 0m);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Add_SentOutsideLab_PatientPriceZero_ShouldPass()
    {
        var cmd = CreateAddCommand(sentOutsideLab: true, labId: 1, costPrice: 50m, patientPrice: 0m);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Add_SentOutsideLab_NegativeMargin_ShouldPass()
    {
        var cmd = CreateAddCommand(sentOutsideLab: true, labId: 1, costPrice: 100m, patientPrice: 50m);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Add_NotSentOutsideLab_LabIdMustBeNull()
    {
        var cmd = CreateAddCommand(sentOutsideLab: false, labId: 1, costPrice: null);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OutsourcedLabReferralEntityId");
    }

    [Fact]
    public void Add_NotSentOutsideLab_CostPriceMustBeNull()
    {
        var cmd = CreateAddCommand(sentOutsideLab: false, labId: null, costPrice: 50m);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OutsourcedCostPrice");
    }

    [Fact]
    public void Add_PriceZero_ShouldPass()
    {
        var cmd = CreateAddCommand(patientPrice: 0m);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Add_NegativePrice_ShouldFail()
    {
        var cmd = CreateAddCommand(patientPrice: -1m);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Price");
    }

    #endregion

    #region Update — OQ-6: SentOutsideLab validation

    [Fact]
    public void Update_SentOutsideLab_MissingLab_ShouldFail()
    {
        var cmd = CreateUpdateCommand(sentOutsideLab: true, labId: null, costPrice: 50m);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OutsourcedLabReferralEntityId");
    }

    [Fact]
    public void Update_SentOutsideLab_MissingCostPrice_ShouldFail()
    {
        var cmd = CreateUpdateCommand(sentOutsideLab: true, labId: 1, costPrice: null);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OutsourcedCostPrice");
    }

    [Fact]
    public void Update_SentOutsideLab_CostPriceZero_ShouldPass()
    {
        var cmd = CreateUpdateCommand(sentOutsideLab: true, labId: 1, costPrice: 0m, patientPrice: 0m);
        var result = _updateValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_SentOutsideLab_PatientPriceZero_ShouldPass()
    {
        var cmd = CreateUpdateCommand(sentOutsideLab: true, labId: 1, costPrice: 50m, patientPrice: 0m);
        var result = _updateValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_SentOutsideLab_NegativeMargin_ShouldPass()
    {
        var cmd = CreateUpdateCommand(sentOutsideLab: true, labId: 1, costPrice: 100m, patientPrice: 50m);
        var result = _updateValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_NotSentOutsideLab_LabIdMustBeNull()
    {
        var cmd = CreateUpdateCommand(sentOutsideLab: false, labId: 1, costPrice: null);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OutsourcedLabReferralEntityId");
    }

    [Fact]
    public void Update_NotSentOutsideLab_CostPriceMustBeNull()
    {
        var cmd = CreateUpdateCommand(sentOutsideLab: false, labId: null, costPrice: 50m);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OutsourcedCostPrice");
    }

    [Fact]
    public void Update_PriceZero_ShouldPass()
    {
        var cmd = CreateUpdateCommand(patientPrice: 0m);
        var result = _updateValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_NegativePrice_ShouldFail()
    {
        var cmd = CreateUpdateCommand(patientPrice: -1m);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Price");
    }

    #endregion

    private static AddTestCommand CreateAddCommand(
        bool sentOutsideLab = false,
        int? labId = null,
        decimal? costPrice = null,
        decimal patientPrice = 100m)
        => new("CBC", "Report", "Receipt", "Group", null, patientPrice, "1h", false, "mg",
            null, null, null, null, null, null, false, false, false, false, 1, 0,
            ReferenceType.General, null, null, null, null, null,
            sentOutsideLab, null, costPrice, null, 35.50m, labId);

    private static UpdateTestCommand CreateUpdateCommand(
        bool sentOutsideLab = false,
        int? labId = null,
        decimal? costPrice = null,
        decimal patientPrice = 100m)
        => new(1, "CBC", "Report", "Receipt", "Group", null, patientPrice, "1h", false, "mg",
            null, null, null, null, null, null, false, false, false, false, 1, 0,
            ReferenceType.General, null, null, null, null, null,
            sentOutsideLab, null, costPrice, null, 35.50m, labId);
}
