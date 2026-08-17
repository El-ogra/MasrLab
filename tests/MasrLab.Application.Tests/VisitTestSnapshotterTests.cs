using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Application.Tests;

public class VisitTestSnapshotterTests
{
    private readonly VisitTestSnapshotter _snapshotter = new();

    [Fact]
    public void CreateVisitTestSnapshot_WithMultipleComponents_CreatesSnapshotWithAllComponents()
    {
        var test = new Test
        {
            Id = 10,
            Name = "CBC Test",
            ReportName = "CBC Report",
            ReceiptName = "CBC Receipt",
            Price = 150m
        };
        test.TestComponents.Add(new TestComponent
        {
            Id = 101, TestId = 10, Name = "WBC", Unit = "10^3/uL",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });
        test.TestComponents.Add(new TestComponent
        {
            Id = 102, TestId = 10, Name = "RBC", Unit = "10^6/uL",
            DisplayOrder = 2, ResultEntryKind = ResultEntryKind.Ordinary
        });
        test.TestComponents.Add(new TestComponent
        {
            Id = 103, TestId = 10, Name = "HGB", Unit = "g/dL",
            DisplayOrder = 3, ResultEntryKind = ResultEntryKind.Ordinary
        });

        var (visitTest, resultItems) = _snapshotter.CreateVisitTestSnapshot(
            test, visitId: 1, price: 150m, isOutsourced: false);

        Assert.Equal(10, visitTest.TestId);
        Assert.Equal(1, visitTest.PatientVisitId);
        Assert.Equal(150m, visitTest.Price);
        Assert.False(visitTest.IsOutsourced);
        Assert.Equal("CBC Test", visitTest.TestNameSnapshot);
        Assert.Equal("CBC Report", visitTest.ReportNameSnapshot);
        Assert.Equal("CBC Receipt", visitTest.ReceiptNameSnapshot);
        Assert.True(visitTest.IsCompoundSnapshot);
        Assert.Equal(3, resultItems.Count);
        Assert.Equal(3, visitTest.ResultItems.Count);
    }

    [Fact]
    public void CreateVisitTestSnapshot_WithSingleComponent_SetsIsCompoundFalse()
    {
        var test = new Test
        {
            Id = 20,
            Name = "Glucose Test",
            ReportName = "Glucose Report",
            ReceiptName = "Glucose Receipt",
            Price = 50m
        };
        test.TestComponents.Add(new TestComponent
        {
            Id = 201, TestId = 20, Name = "Glucose", Unit = "mg/dL",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });

        var (visitTest, resultItems) = _snapshotter.CreateVisitTestSnapshot(
            test, visitId: 2, price: 50m, isOutsourced: true);

        Assert.False(visitTest.IsCompoundSnapshot);
        Assert.True(visitTest.IsOutsourced);
        Assert.Single(resultItems);
        Assert.Equal("Glucose", resultItems[0].ComponentName);
        Assert.Equal("mg/dL", resultItems[0].ComponentUnit);
        Assert.Equal(1, resultItems[0].DisplayOrder);
        Assert.Equal(ResultEntryKind.Ordinary, resultItems[0].ResultEntryKind);
    }

    [Fact]
    public void CreateVisitTestSnapshot_WithZeroComponents_ThrowsBusinessRuleViolation()
    {
        var test = new Test
        {
            Id = 30,
            Name = "Draft Test",
            TestComponents = new List<TestComponent>()
        };

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => _snapshotter.CreateVisitTestSnapshot(test, visitId: 1, price: 100m, isOutsourced: false));

        Assert.Contains("draft", ex.Message);
    }

    [Fact]
    public void CreateVisitTestSnapshot_PreservesComponentOrder()
    {
        var test = new Test { Id = 40, Name = "Ordered Test" };
        test.TestComponents.Add(new TestComponent
        {
            Id = 403, TestId = 40, Name = "Third", Unit = "U3",
            DisplayOrder = 3, ResultEntryKind = ResultEntryKind.Ordinary
        });
        test.TestComponents.Add(new TestComponent
        {
            Id = 401, TestId = 40, Name = "First", Unit = "U1",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });
        test.TestComponents.Add(new TestComponent
        {
            Id = 402, TestId = 40, Name = "Second", Unit = "U2",
            DisplayOrder = 2, ResultEntryKind = ResultEntryKind.Ordinary
        });

        var (visitTest, resultItems) = _snapshotter.CreateVisitTestSnapshot(
            test, visitId: 3, price: 200m, isOutsourced: false);

        Assert.Equal(3, resultItems.Count);
        Assert.Equal("First", resultItems[0].ComponentName);
        Assert.Equal("Second", resultItems[1].ComponentName);
        Assert.Equal("Third", resultItems[2].ComponentName);
        Assert.Equal(1, resultItems[0].DisplayOrder);
        Assert.Equal(2, resultItems[1].DisplayOrder);
        Assert.Equal(3, resultItems[2].DisplayOrder);
    }

    [Fact]
    public void CreateVisitTestSnapshot_SetsSourceTestComponentId()
    {
        var test = new Test { Id = 50, Name = "ComponentId Test" };
        test.TestComponents.Add(new TestComponent
        {
            Id = 501, TestId = 50, Name = "Comp1", Unit = "U1",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });
        test.TestComponents.Add(new TestComponent
        {
            Id = 502, TestId = 50, Name = "Comp2", Unit = "U2",
            DisplayOrder = 2, ResultEntryKind = ResultEntryKind.CultureDetail
        });

        var (visitTest, resultItems) = _snapshotter.CreateVisitTestSnapshot(
            test, visitId: 4, price: 75m, isOutsourced: false);

        Assert.Equal(501, resultItems[0].SourceTestComponentId);
        Assert.Equal(502, resultItems[1].SourceTestComponentId);
        Assert.Equal(ResultEntryKind.Ordinary, resultItems[0].ResultEntryKind);
        Assert.Equal(ResultEntryKind.CultureDetail, resultItems[1].ResultEntryKind);
    }

    [Fact]
    public void CreateVisitTestSnapshot_CultureDetailKind_Preserved()
    {
        var test = new Test { Id = 60, Name = "Culture Test" };
        test.TestComponents.Add(new TestComponent
        {
            Id = 601, TestId = 60, Name = "Culture", Unit = "",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.CultureDetail
        });

        var (visitTest, resultItems) = _snapshotter.CreateVisitTestSnapshot(
            test, visitId: 5, price: 300m, isOutsourced: false);

        Assert.Single(resultItems);
        Assert.Equal(ResultEntryKind.CultureDetail, resultItems[0].ResultEntryKind);
    }

    [Fact]
    public void CreateVisitTestSnapshot_WithOneActiveAndOneDeletedComponent_ExcludesDeletedComponent()
    {
        var test = new Test { Id = 70, Name = "Mixed Test" };
        test.TestComponents.Add(new TestComponent
        {
            Id = 701, TestId = 70, Name = "Active", Unit = "U1",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });
        var deletedComponent = new TestComponent
        {
            Id = 702, TestId = 70, Name = "Deleted", Unit = "U2",
            DisplayOrder = 2, ResultEntryKind = ResultEntryKind.Ordinary
        };
        deletedComponent.IsDeleted = true;
        test.TestComponents.Add(deletedComponent);

        var (visitTest, resultItems) = _snapshotter.CreateVisitTestSnapshot(
            test, visitId: 6, price: 100m, isOutsourced: false);

        Assert.Single(resultItems);
        Assert.Equal(701, resultItems[0].SourceTestComponentId);
        Assert.False(visitTest.IsCompoundSnapshot);
    }

    [Fact]
    public void CreateVisitTestSnapshot_WithAllDeletedComponents_ThrowsBusinessRuleViolation()
    {
        var test = new Test { Id = 80, Name = "All Deleted Test" };
        var comp1 = new TestComponent
        {
            Id = 801, TestId = 80, Name = "Del1", Unit = "U1",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        };
        comp1.IsDeleted = true;
        var comp2 = new TestComponent
        {
            Id = 802, TestId = 80, Name = "Del2", Unit = "U2",
            DisplayOrder = 2, ResultEntryKind = ResultEntryKind.Ordinary
        };
        comp2.IsDeleted = true;
        test.TestComponents.Add(comp1);
        test.TestComponents.Add(comp2);

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => _snapshotter.CreateVisitTestSnapshot(test, visitId: 7, price: 50m, isOutsourced: false));

        Assert.Contains("draft", ex.Message);
    }
}
