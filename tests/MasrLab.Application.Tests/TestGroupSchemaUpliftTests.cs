using MasrLab.Domain.Entities.Core;

namespace MasrLab.Application.Tests;

public class TestGroupSchemaUpliftTests
{
    [Fact]
    public void TotalGroupPrice_SumsNonDeletedItemPrices()
    {
        var group = new TestGroup { GroupName = "Checkup" };
        group.TestGroupItems.Add(new TestGroupItem { TestGroupId = 1, TestId = 1, Price = 50m, DisplayOrder = 1 });
        group.TestGroupItems.Add(new TestGroupItem { TestGroupId = 1, TestId = 2, Price = 30m, DisplayOrder = 2 });
        group.TestGroupItems.Add(new TestGroupItem { TestGroupId = 1, TestId = 3, Price = 10m, DisplayOrder = 3, IsDeleted = true });

        Assert.Equal(80m, group.TotalGroupPrice);
    }

    [Fact]
    public void TotalGroupPrice_ReturnsZeroForEmptyGroup()
    {
        var group = new TestGroup { GroupName = "Empty" };
        Assert.Equal(0m, group.TotalGroupPrice);
    }

    [Fact]
    public void TestGroupItem_Price_DefaultsToZero()
    {
        var item = new TestGroupItem { TestGroupId = 1, TestId = 1, DisplayOrder = 1 };
        Assert.Equal(0m, item.Price);
    }
}
