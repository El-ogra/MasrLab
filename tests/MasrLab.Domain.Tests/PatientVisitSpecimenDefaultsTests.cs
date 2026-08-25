using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Tests;

public class PatientVisitSpecimenDefaultsTests
{
    [Fact]
    public void Create_StartsWithTakenOutsideLabAndSpecimenFlagsFalse()
    {
        var visit = PatientVisit.Create(
            patientId: 1,
            registeredByUserId: 2,
            labId: "LAB-0001",
            doctorId: null,
            referralEntityId: null);

        Assert.False(visit.TakenOutsideLab);
        Assert.False(visit.SpecimenUrine);
        Assert.False(visit.SpecimenStool);
        Assert.False(visit.SpecimenBlood);
        Assert.False(visit.SpecimenSemen);
        Assert.False(visit.SpecimenCsf);
    }
}
