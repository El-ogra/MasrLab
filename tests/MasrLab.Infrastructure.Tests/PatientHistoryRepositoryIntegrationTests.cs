using MasrLab.Domain.Common.Enums;
using MasrLab.Infrastructure.Persistence.Repositories;

namespace MasrLab.Infrastructure.Tests;

public sealed class PatientHistoryRepositoryIntegrationTests
{
    private const string DatabasePrefix = "MasrLabDb_PatientHistory";

    [LocalDbFact]
    public async Task GetByPatientIdAsync_ReturnsOnlyPatientRows_OrderedByTestThenCurrentVisitDate()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();
        var data = new PatientHistoryRepositoryTestData(context);
        var patient = await data.AddPatientAsync("Target patient");
        var otherPatient = await data.AddPatientAsync("Other patient");
        var firstTest = await data.AddTestAsync("First test");
        var secondTest = await data.AddTestAsync("Second test");

        var firstDate = new DateTime(2026, 1, 10);
        var secondDate = new DateTime(2026, 1, 12);
        var thirdDate = new DateTime(2026, 1, 11);
        await data.AddResultAsync(patient, firstTest, secondDate, "6.0");
        await data.AddResultAsync(patient, secondTest, thirdDate, "8.0");
        await data.AddResultAsync(patient, firstTest, firstDate, "5.0");
        await data.AddResultAsync(otherPatient, firstTest, firstDate, "99.0");

        var result = await new PatientHistoryRepository(context).GetByPatientIdAsync(patient.Id);

        Assert.Equal(3, result.Count);
        Assert.All(result, entry => Assert.Equal(patient.Id, entry.PatientId));
        Assert.Equal(
            new[] { (firstTest.Id, firstDate), (firstTest.Id, secondDate), (secondTest.Id, thirdDate) },
            result.Select(entry => (entry.TestId, entry.CurrentVisitDate)));
    }

    [LocalDbFact]
    public async Task GetByPatientAndTestAsync_FiltersByBothIds_AndOrdersByCurrentVisitDate()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();
        var data = new PatientHistoryRepositoryTestData(context);
        var patient = await data.AddPatientAsync("Target patient");
        var otherPatient = await data.AddPatientAsync("Other patient");
        var targetTest = await data.AddTestAsync("Target test");
        var otherTest = await data.AddTestAsync("Other test");
        var firstDate = new DateTime(2026, 2, 1);
        var secondDate = new DateTime(2026, 2, 3);

        await data.AddResultAsync(patient, targetTest, secondDate, "7.0");
        await data.AddResultAsync(patient, targetTest, firstDate, "6.0");
        await data.AddResultAsync(patient, otherTest, firstDate, "8.0");
        await data.AddResultAsync(otherPatient, targetTest, firstDate, "9.0");

        var result = await new PatientHistoryRepository(context).GetByPatientAndTestAsync(patient.Id, targetTest.Id);

        Assert.Equal(2, result.Count);
        Assert.All(result, entry =>
        {
            Assert.Equal(patient.Id, entry.PatientId);
            Assert.Equal(targetTest.Id, entry.TestId);
        });
        Assert.Equal(new[] { firstDate, secondDate }, result.Select(entry => entry.CurrentVisitDate));
    }

    [LocalDbFact]
    public async Task GetByPatientIdAsync_UsesViewComparisonFlag_ForClinicalDifferences_AndFalseForBaseline()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();
        var data = new PatientHistoryRepositoryTestData(context);
        var patient = await data.AddPatientAsync("Comparison patient");
        var unchanged = await data.AddTestAsync("Unchanged");
        var valueChanged = await data.AddTestAsync("Value changed");
        var unitChanged = await data.AddTestAsync("Unit changed");
        var rangeChanged = await data.AddTestAsync("Range changed");
        var previous = new DateTime(2026, 3, 1);
        var current = new DateTime(2026, 3, 2);

        await AddPairAsync(unchanged, "5", "mg/dL", "1-10", "5", "mg/dL", "1-10");
        await AddPairAsync(valueChanged, "5", "mg/dL", "1-10", "6", "mg/dL", "1-10");
        await AddPairAsync(unitChanged, "5", "mg/dL", "1-10", "5", "mmol/L", "1-10");
        await AddPairAsync(rangeChanged, "5", "mg/dL", "1-10", "5", "mg/dL", "2-10");

        var result = await new PatientHistoryRepository(context).GetByPatientIdAsync(patient.Id);

        Assert.False(CurrentFor(unchanged).ComparisonFlag);
        Assert.True(CurrentFor(valueChanged).ComparisonFlag);
        Assert.True(CurrentFor(unitChanged).ComparisonFlag);
        Assert.True(CurrentFor(rangeChanged).ComparisonFlag);
        Assert.All(result.Where(entry => entry.CurrentVisitDate == previous), entry => Assert.False(entry.ComparisonFlag));

        async Task AddPairAsync(MasrLab.Domain.Entities.Core.Test test, string previousValue, string previousUnit, string previousRange, string currentValue, string currentUnit, string currentRange)
        {
            await data.AddResultAsync(patient, test, previous, previousValue, previousUnit, previousRange, ResultStatus.Normal);
            await data.AddResultAsync(patient, test, current, currentValue, currentUnit, currentRange, ResultStatus.High);
        }

        MasrLab.Domain.Common.DTOs.PatientHistoryEntry CurrentFor(MasrLab.Domain.Entities.Core.Test test) =>
            Assert.Single(result.Where(entry => entry.TestId == test.Id && entry.CurrentVisitDate == current));
    }
}
