using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence.Repositories;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
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
        var firstComponent = await data.AddTestAsync("First test");
        var secondComponent = await data.AddTestAsync("Second test");

        var firstDate = new DateTime(2026, 1, 10);
        var secondDate = new DateTime(2026, 1, 12);
        var thirdDate = new DateTime(2026, 1, 11);
        await data.AddResultAsync(patient, firstComponent, secondDate, "6.0");
        await data.AddResultAsync(patient, secondComponent, thirdDate, "8.0");
        await data.AddResultAsync(patient, firstComponent, firstDate, "5.0");
        await data.AddResultAsync(otherPatient, firstComponent, firstDate, "99.0");

        var result = await new PatientHistoryRepository(context).GetByPatientIdAsync(patient.Id);

        Assert.Equal(3, result.Count);
        Assert.All(result, entry => Assert.Equal(patient.Id, entry.PatientId));
        Assert.Equal(
            new[] { (firstComponent.TestId, firstDate), (firstComponent.TestId, secondDate), (secondComponent.TestId, thirdDate) },
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
        var targetComponent = await data.AddTestAsync("Target test");
        var otherComponent = await data.AddTestAsync("Other test");
        var firstDate = new DateTime(2026, 2, 1);
        var secondDate = new DateTime(2026, 2, 3);

        await data.AddResultAsync(patient, targetComponent, secondDate, "7.0");
        await data.AddResultAsync(patient, targetComponent, firstDate, "6.0");
        await data.AddResultAsync(patient, otherComponent, firstDate, "8.0");
        await data.AddResultAsync(otherPatient, targetComponent, firstDate, "9.0");

        var result = await new PatientHistoryRepository(context).GetByPatientAndTestAsync(patient.Id, targetComponent.TestId);

        Assert.Equal(2, result.Count);
        Assert.All(result, entry =>
        {
            Assert.Equal(patient.Id, entry.PatientId);
            Assert.Equal(targetComponent.TestId, entry.TestId);
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
        var unchangedComponent = await data.AddTestAsync("Unchanged");
        var valueChangedComponent = await data.AddTestAsync("Value changed");
        var unitChangedComponent = await data.AddTestAsync("Unit changed");
        var rangeChangedComponent = await data.AddTestAsync("Range changed");
        var previous = new DateTime(2026, 3, 1);
        var current = new DateTime(2026, 3, 2);

        await AddPairAsync(unchangedComponent, "5", "mg/dL", "1-10", "5", "mg/dL", "1-10");
        await AddPairAsync(valueChangedComponent, "5", "mg/dL", "1-10", "6", "mg/dL", "1-10");
        await AddPairAsync(unitChangedComponent, "5", "mg/dL", "1-10", "5", "mmol/L", "1-10");
        await AddPairAsync(rangeChangedComponent, "5", "mg/dL", "1-10", "5", "mg/dL", "2-10");

        var result = await new PatientHistoryRepository(context).GetByPatientIdAsync(patient.Id);

        Assert.False(CurrentFor(unchangedComponent).ComparisonFlag);
        Assert.True(CurrentFor(valueChangedComponent).ComparisonFlag);
        Assert.True(CurrentFor(unitChangedComponent).ComparisonFlag);
        Assert.True(CurrentFor(rangeChangedComponent).ComparisonFlag);
        Assert.All(result.Where(entry => entry.CurrentVisitDate == previous), entry => Assert.False(entry.ComparisonFlag));

        async Task AddPairAsync(TestComponent component, string previousValue, string previousUnit, string previousRange, string currentValue, string currentUnit, string currentRange)
        {
            await data.AddResultAsync(patient, component, previous, previousValue, previousUnit, previousRange, ResultStatus.Normal);
            await data.AddResultAsync(patient, component, current, currentValue, currentUnit, currentRange, ResultStatus.High);
        }

        MasrLab.Domain.Common.DTOs.PatientHistoryEntry CurrentFor(TestComponent component) =>
            Assert.Single(result.Where(entry => entry.TestId == component.TestId && entry.CurrentVisitDate == current));
    }
}
