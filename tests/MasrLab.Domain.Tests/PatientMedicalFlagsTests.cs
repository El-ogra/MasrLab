using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Tests;

public class PatientMedicalFlagsTests
{
    [Fact]
    public void Register_defaults_all_twelve_medical_flags_to_false()
    {
        var patient = Patient.Register("Ahmed", "LAB-001");

        Assert.False(patient.HasDiabetes);
        Assert.False(patient.OnBloodPressureTreatment);
        Assert.False(patient.OnAntiviralTreatment);
        Assert.False(patient.OnAntibiotic);
        Assert.False(patient.BloodThinning);
        Assert.False(patient.HasLiverDisease);
        Assert.False(patient.HasAnemia);
        Assert.False(patient.HasLupus);
        Assert.False(patient.HasRenalFailure);
        Assert.False(patient.HasHypertension);
        Assert.False(patient.HasJointDisease);
        Assert.False(patient.RecentContrastOrUltrasound);
    }

    [Fact]
    public void Each_medical_flag_can_be_set_independently()
    {
        var cases = new (Action<Patient> Set, Func<Patient, bool> Read)[]
        {
            (p => p.HasDiabetes = true, p => p.HasDiabetes),
            (p => p.OnBloodPressureTreatment = true, p => p.OnBloodPressureTreatment),
            (p => p.OnAntiviralTreatment = true, p => p.OnAntiviralTreatment),
            (p => p.OnAntibiotic = true, p => p.OnAntibiotic),
            (p => p.BloodThinning = true, p => p.BloodThinning),
            (p => p.HasLiverDisease = true, p => p.HasLiverDisease),
            (p => p.HasAnemia = true, p => p.HasAnemia),
            (p => p.HasLupus = true, p => p.HasLupus),
            (p => p.HasRenalFailure = true, p => p.HasRenalFailure),
            (p => p.HasHypertension = true, p => p.HasHypertension),
            (p => p.HasJointDisease = true, p => p.HasJointDisease),
            (p => p.RecentContrastOrUltrasound = true, p => p.RecentContrastOrUltrasound)
        };

        foreach (var (set, read) in cases)
        {
            var patient = Patient.Register("Ahmed", Guid.NewGuid().ToString("N"));
            set(patient);
            Assert.True(read(patient));
        }
    }
}
