using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Exceptions;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class LabIdConcurrencyIntegrationTests
{
    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public int? UserId => 1;
        public string? Username => "integration-test";
        public string? Role => "LabTechnician";
    }

    private const int ConcurrentRegistrations = 12;
    private const string InitialLabId = "20260101-0001";

    [LocalDbFact]
    public async Task ConcurrentRegistrations_ProduceUniqueLabIds_WithReasonableSuccessRate()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Concurrency");

        // Apply migrations once up front so the parallel tasks never race on schema creation.
        using (var bootstrap = LocalDbTestDatabase.CreateMigratedContext(databaseName))
        {
        }

        var startGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tasks = Enumerable.Range(0, ConcurrentRegistrations)
            .Select(_ => Task.Run(async () =>
            {
                await startGate.Task;

                // Each registration uses its own DbContext/scope — contexts are never shared
                // between concurrent tasks (required by EF Core and by the plan).
                using var context = LocalDbTestDatabase.CreateContext(databaseName);
                var repository = new PatientRepository(context);
                var unitOfWork = new UnitOfWork(context);
                var generator = new LabIdGenerator(repository);
                var currentUserService = new TestCurrentUserService();
                var handler = new RegisterPatientCommandHandler(repository, unitOfWork, generator, currentUserService);

                try
                {
                    await handler.Handle(CreateCommand(InitialLabId), CancellationToken.None);
                    return 1; // success
                }
                catch (DuplicateLabIdException)
                {
                    // Limited-retry policy (5 attempts + backoff/jitter) is the accepted final
                    // behaviour for this round (Decision 2): some requests may exhaust it.
                    return 0;
                }
            }))
            .ToArray();

        startGate.SetResult();
        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r == 1);

        using (var readContext = LocalDbTestDatabase.CreateContext(databaseName))
        {
            var labIds = await readContext.Patients
                .Select(p => p.LabId)
                .ToListAsync(CancellationToken.None);

            Assert.Equal(successCount, labIds.Count);
            Assert.Equal(labIds.Count, labIds.Distinct().Count());
        }

        Assert.True(successCount >= 1, "At least one registration should succeed.");
        Assert.True(
            successCount >= ConcurrentRegistrations / 2,
            $"Expected a reasonable success rate (at least {ConcurrentRegistrations / 2} of {ConcurrentRegistrations}) " +
            $"within the 5-attempt retry limit, but only {successCount} succeeded.");

        await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
        await cleanup.Database.EnsureDeletedAsync();
    }

    private static RegisterPatientCommand CreateCommand(string labId) => new(
        "Concurrent Patient", 30, 0, 0, AgeUnit.Years, Gender.Male, null, null, null, null,
        labId, null, null);
}
