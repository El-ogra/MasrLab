using MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;
using MasrLab.Application.Features.DoctorsAndReferrals.Commands.UpdateReferralEntity;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

public class Module12_ReferralEntityValidatorTests
{
    private readonly AddReferralEntityCommandValidator _addValidator = new();
    private readonly UpdateReferralEntityCommandValidator _updateValidator = new();

    #region Add — TreatingDoctor

    [Fact]
    public void Add_TreatingDoctor_WithPriceListId_ShouldFail()
    {
        var cmd = new AddReferralEntityCommand("Dr", ReferralEntityType.TreatingDoctor, null, null, null, null, null, null, null, 1);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PriceListId");
    }

    [Fact]
    public void Add_TreatingDoctor_WithoutPriceList_ShouldPass()
    {
        var cmd = new AddReferralEntityCommand("Dr", ReferralEntityType.TreatingDoctor, null, null, null, null, null, null, null, null);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Add_TreatingDoctor_NegativeDiscount_ShouldFail()
    {
        var cmd = new AddReferralEntityCommand("Dr", ReferralEntityType.TreatingDoctor, null, null, null, null, null, -1m, null, null);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Discount");
    }

    [Fact]
    public void Add_TreatingDoctor_NegativeCommission_ShouldFail()
    {
        var cmd = new AddReferralEntityCommand("Dr", ReferralEntityType.TreatingDoctor, null, null, null, null, null, null, -5m, null);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Commission");
    }

    [Fact]
    public void Add_TreatingDoctor_ZeroDiscount_ShouldPass()
    {
        var cmd = new AddReferralEntityCommand("Dr", ReferralEntityType.TreatingDoctor, null, null, null, null, null, 0m, 0m, null);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    #endregion

    #region Add — ReferralEntity

    [Fact]
    public void Add_ReferralEntity_WithoutPriceList_ShouldFail()
    {
        var cmd = new AddReferralEntityCommand("Clinic", ReferralEntityType.ReferralEntity, null, null, null, null, null, null, null, null);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PriceListId");
    }

    [Fact]
    public void Add_ReferralEntity_WithPriceList_ShouldPass()
    {
        var cmd = new AddReferralEntityCommand("Clinic", ReferralEntityType.ReferralEntity, null, null, null, null, null, null, null, 1);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    #endregion

    #region Add — OutsourcedSamples (D-02: PriceListId not validated by handler enforces Lab-to-Lab lock)

    [Fact]
    public void Add_OutsourceSamples_WithoutPriceList_ShouldPass_ValidatorDoesNotEnforce()
    {
        var cmd = new AddReferralEntityCommand("Lab", ReferralEntityType.OutsourcedSamples, null, null, null, null, null, null, null, null);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Add_OutsourceSamples_WithPriceList_ShouldPass()
    {
        var cmd = new AddReferralEntityCommand("Lab", ReferralEntityType.OutsourcedSamples, null, null, null, null, null, null, null, 1);
        var result = _addValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    #endregion

    #region Add — Name

    [Fact]
    public void Add_EmptyName_ShouldFail()
    {
        var cmd = new AddReferralEntityCommand("", ReferralEntityType.TreatingDoctor, null, null, null, null, null, null, null, null);
        var result = _addValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    #endregion

    #region Update — Common rules

    [Fact]
    public void Update_ZeroId_ShouldFail()
    {
        var cmd = new UpdateReferralEntityCommand(0, "Dr", null, null, null, null, null, null, null, null);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void Update_EmptyName_ShouldFail()
    {
        var cmd = new UpdateReferralEntityCommand(1, "", null, null, null, null, null, null, null, null);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Update_NegativeDiscount_ShouldFail()
    {
        var cmd = new UpdateReferralEntityCommand(1, "Dr", null, null, null, null, null, -1m, null, null);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Discount");
    }

    [Fact]
    public void Update_NegativeCommission_ShouldFail()
    {
        var cmd = new UpdateReferralEntityCommand(1, "Dr", null, null, null, null, null, null, -5m, null);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Commission");
    }

    [Fact]
    public void Update_NullDiscount_ShouldPass()
    {
        var cmd = new UpdateReferralEntityCommand(1, "Dr", null, null, null, null, null, null, null, null);
        var result = _updateValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_ZeroPriceListId_ShouldFail()
    {
        var cmd = new UpdateReferralEntityCommand(1, "Clinic", null, null, null, null, null, null, null, 0);
        var result = _updateValidator.Validate(cmd);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PriceListId");
    }

    [Fact]
    public void Update_NullPriceListId_ShouldPass()
    {
        var cmd = new UpdateReferralEntityCommand(1, "Dr", null, null, null, null, null, null, null, null);
        var result = _updateValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_ValidReferralEntity_ShouldPass()
    {
        var cmd = new UpdateReferralEntityCommand(1, "Clinic", "Sara", "01012345678", "fax", "Giza", "Cairo", 10m, 5m, 2);
        var result = _updateValidator.Validate(cmd);
        Assert.True(result.IsValid);
    }

    #endregion
}
