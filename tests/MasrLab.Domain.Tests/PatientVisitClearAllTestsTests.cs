using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class PatientVisitClearAllTestsTests
{
    [Fact]
    public void ClearAllTests_OnNewVisit_ClearsTestsSamplesAndPromisedDelivery()
    {
        var visit = PatientVisit.Create(1, 2, "20260825-0001", null, null);
        visit.AddVisitTest(new VisitTest(0, 10, 100m, false));
        visit.Samples.Add(Sample.Create(0, 10));
        visit.ExtendPromisedDelivery(3);

        visit.ClearAllTests();

        Assert.Empty(visit.VisitTests);
        Assert.Empty(visit.Samples);
        Assert.Null(visit.PromisedDeliveryAt);
    }

    [Fact]
    public void ClearAllTests_OnPersistedVisit_Throws()
    {
        var visit = PatientVisit.Create(1, 2, "20260825-0001", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!.SetValue(visit, 1);

        Assert.Throws<BusinessRuleViolationException>(() => visit.ClearAllTests());
    }
}
