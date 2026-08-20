using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class PatientVisitPromisedDeliveryTests
{
    [Fact]
    public void ExtendPromisedDelivery_StoresVisitDatePlusTat()
    {
        var visit = PatientVisit.Create(1, 1, "20260820-0001", null, null);

        visit.ExtendPromisedDelivery(5);

        Assert.Equal(visit.VisitDate.AddDays(5), visit.PromisedDeliveryAt);
    }

    [Fact]
    public void ExtendPromisedDelivery_KeepsTheLongestExistingDeadline()
    {
        var visit = PatientVisit.Create(1, 1, "20260820-0001", null, null);
        visit.ExtendPromisedDelivery(5);

        visit.ExtendPromisedDelivery(2);

        Assert.Equal(visit.VisitDate.AddDays(5), visit.PromisedDeliveryAt);
    }

    [Fact]
    public void ExtendPromisedDelivery_ExtendsWhenNewTatIsLonger()
    {
        var visit = PatientVisit.Create(1, 1, "20260820-0001", null, null);
        visit.ExtendPromisedDelivery(2);

        visit.ExtendPromisedDelivery(7);

        Assert.Equal(visit.VisitDate.AddDays(7), visit.PromisedDeliveryAt);
    }

    [Fact]
    public void ExtendPromisedDelivery_RejectsNegativeTat()
    {
        var visit = PatientVisit.Create(1, 1, "20260820-0001", null, null);

        Assert.Throws<BusinessRuleViolationException>(() => visit.ExtendPromisedDelivery(-1));
    }
}
