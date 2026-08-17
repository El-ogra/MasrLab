using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class OutsourcedSampleRepositoryDateRangeIntegrationTests
{
    private const string DatabasePrefix = "MasrLabDb_OutsourcedDateRange";

    [LocalDbFact]
    public async Task GetByReceivedDateRangeAsync_IncludesBoundaries_ExcludesOutsideAndNull()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName(DatabasePrefix);
        using var context = LocalDbTestDatabase.CreateCreatedContext(databaseName);
        try
        {
            var start = new DateTime(2026, 2, 10);
            var end = new DateTime(2026, 2, 20);

            var visit = PatientVisit.Create(patientId: 1, registeredByUserId: 1, labId: "OS-1", doctorId: null, referralEntityId: null);
            context.PatientVisits.Add(visit);
            await context.SaveChangesAsync(CancellationToken.None);

            context.OutsourcedSamples.AddRange(
                CreateSample(externalLabId: 1, new DateTime(2026, 2, 9)),
                CreateSample(externalLabId: 2, start),
                CreateSample(externalLabId: 3, new DateTime(2026, 2, 15)),
                CreateSample(externalLabId: 4, end),
                CreateSample(externalLabId: 5, new DateTime(2026, 2, 21)),
                CreateSample(externalLabId: 6, receivedAt: null));
            await context.SaveChangesAsync(CancellationToken.None);

            var repository = new OutsourcedSampleRepository(context);
            var result = await repository.GetByReceivedDateRangeAsync(start, end, CancellationToken.None);

            Assert.Equal(3, result.Count);
            Assert.Equal(
                new[] { 2, 3, 4 }.OrderBy(x => x),
                result.Select(s => s.ExternalLabId).OrderBy(x => x));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task GetByReceivedDateRangeAsync_ReturnsEmpty_WhenNoSamplesInRange()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName(DatabasePrefix);
        using var context = LocalDbTestDatabase.CreateCreatedContext(databaseName);
        try
        {
            var repository = new OutsourcedSampleRepository(context);
            var result = await repository.GetByReceivedDateRangeAsync(
                new DateTime(2026, 2, 10), new DateTime(2026, 2, 20), CancellationToken.None);

            Assert.Empty(result);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static OutsourcedSample CreateSample(int externalLabId, DateTime? receivedAt)
    {
        return new OutsourcedSample
        {
            PatientVisitId = 1,
            TestId = 1,
            ExternalLabId = externalLabId,
            ReceivedAt = receivedAt
        };
    }
}
