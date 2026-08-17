using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class PatientVisitAddVisitTestTests
{
    [Fact]
    public void AddVisitTest_WhenRegistered_ShouldAddAndRaiseEvent()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        var visitTest = new VisitTest(visit.Id, 10, 100m, false)
        {
            TestNameSnapshot = "Test10"
        };

        visit.AddVisitTest(visitTest);

        var vt = Assert.Single(visit.VisitTests);
        Assert.Equal(10, vt.TestId);
        Assert.Equal(100m, vt.Price);
        Assert.Equal(VisitStatus.Registered, visit.Status);

        var evt = Assert.Single(visit.DomainEvents.OfType<VisitTestAdded>());
        Assert.Equal(visit.Id, evt.VisitId);
        Assert.Equal(10, evt.TestId);
        Assert.Equal(100m, evt.Price);
        Assert.False(evt.IsOutsourced);
    }

    [Fact]
    public void AddVisitTest_WhenResultsEntered_ShouldTransitionToRegistered()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(visit.Id, 1, 100m, false));
        visit.EnterAllResults();
        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);

        var visitTest = new VisitTest(visit.Id, 20, 200m, false);

        visit.AddVisitTest(visitTest);

        Assert.Equal(VisitStatus.Registered, visit.Status);
        Assert.Equal(2, visit.VisitTests.Count);
    }

    [Fact]
    public void AddVisitTest_WhenClosed_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.Close(0m);

        var visitTest = new VisitTest(visit.Id, 10, 100m, false);

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => visit.AddVisitTest(visitTest));

        Assert.Contains("printed or closed", ex.Message);
    }

    [Fact]
    public void AddVisitTest_WithResultItems_ShouldPreserveResultItems()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        var visitTest = new VisitTest(visit.Id, 10, 100m, false);
        visitTest.ResultItems.Add(new VisitTestResultItem
        {
            VisitTestId = visitTest.Id,
            SourceTestComponentId = 101,
            ComponentName = "WBC",
            ComponentUnit = "10^3/uL",
            DisplayOrder = 1,
            ResultEntryKind = ResultEntryKind.Ordinary
        });

        visit.AddVisitTest(visitTest);

        var vt = Assert.Single(visit.VisitTests);
        Assert.Single(vt.ResultItems);
        Assert.Equal("WBC", vt.ResultItems.First().ComponentName);
    }

    [Fact]
    public void AddVisitTest_WhenPrinted_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(visit.Id, 1, 100m, false));
        visit.EnterAllResults();
        visit.MarkAsPrinted();
        Assert.Equal(VisitStatus.Printed, visit.Status);

        var visitTest = new VisitTest(visit.Id, 20, 200m, false);

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => visit.AddVisitTest(visitTest));

        Assert.Contains("printed or closed", ex.Message);
    }
}
