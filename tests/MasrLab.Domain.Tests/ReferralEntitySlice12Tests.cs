using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Domain.Tests;

public class ReferralEntitySlice12Tests
{
    [Fact]
    public void Constructor_WhenCreatedWithDefaults_ShouldNotThrow()
    {
        var entity = new ReferralEntity();

        Assert.Equal(string.Empty, entity.Name);
        Assert.Null(entity.City);
        Assert.Null(entity.Discount);
        Assert.Null(entity.Commission);
        Assert.Null(entity.PriceListId);
        Assert.Null(entity.PriceList);
    }

    [Fact]
    public void City_WhenNull_ShouldAcceptNull()
    {
        var entity = new ReferralEntity { City = null };

        Assert.Null(entity.City);
    }

    [Fact]
    public void City_WhenSet_ShouldStoreValue()
    {
        var entity = new ReferralEntity { City = "Cairo" };

        Assert.Equal("Cairo", entity.City);
    }

    [Fact]
    public void Discount_WhenNull_ShouldAcceptNull()
    {
        var entity = new ReferralEntity { Discount = null };

        Assert.Null(entity.Discount);
    }

    [Fact]
    public void Discount_WhenSetToZero_ShouldStoreZero()
    {
        var entity = new ReferralEntity { Discount = 0m };

        Assert.Equal(0m, entity.Discount);
    }

    [Fact]
    public void Discount_WhenSetToPositive_ShouldStoreValue()
    {
        var entity = new ReferralEntity { Discount = 15.5m };

        Assert.Equal(15.5m, entity.Discount);
    }

    [Fact]
    public void Commission_WhenNull_ShouldAcceptNull()
    {
        var entity = new ReferralEntity { Commission = null };

        Assert.Null(entity.Commission);
    }

    [Fact]
    public void Commission_WhenSetToZero_ShouldStoreZero()
    {
        var entity = new ReferralEntity { Commission = 0m };

        Assert.Equal(0m, entity.Commission);
    }

    [Fact]
    public void Commission_WhenSetToPositive_ShouldStoreValue()
    {
        var entity = new ReferralEntity { Commission = 10m };

        Assert.Equal(10m, entity.Commission);
    }

    [Fact]
    public void PriceList_WhenNull_ShouldAcceptNull()
    {
        var entity = new ReferralEntity { PriceList = null };

        Assert.Null(entity.PriceList);
    }

    [Fact]
    public void PriceList_WhenSet_ShouldStoreReference()
    {
        var priceList = new PriceList { Name = "Standard" };
        var entity = new ReferralEntity { PriceList = priceList };

        Assert.Same(priceList, entity.PriceList);
    }

    [Fact]
    public void AllThreeEntityTypes_CanBeCreatedWithoutBreakingDefaults()
    {
        var doctor = new ReferralEntity { EntityType = ReferralEntityType.TreatingDoctor, Name = "Dr. A" };
        var outsourced = new ReferralEntity { EntityType = ReferralEntityType.OutsourcedSamples, Name = "Lab B" };
        var referral = new ReferralEntity { EntityType = ReferralEntityType.ReferralEntity, Name = "Hospital C" };

        Assert.Equal(ReferralEntityType.TreatingDoctor, doctor.EntityType);
        Assert.Equal(ReferralEntityType.OutsourcedSamples, outsourced.EntityType);
        Assert.Equal(ReferralEntityType.ReferralEntity, referral.EntityType);
    }
}
