using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.ValueObjects;
using MasrLab.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class AccountingRepositoryDateRangeIntegrationTests
{
    private const string DatabasePrefix = "MasrLabDb_AccountingDateRange";

    [LocalDbFact]
    public async Task GetByDateRangeAsync_ReturnsAccountsWhosePeriodIsFullyInsideRange()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName(DatabasePrefix);
        using var context = LocalDbTestDatabase.CreateCreatedContext(databaseName);
        try
        {
            var start = new DateTime(2026, 3, 10);
            var end = new DateTime(2026, 3, 20);

            context.Accounts.AddRange(
                new Account { Period = new DateRange(new DateTime(2026, 3, 11), new DateTime(2026, 3, 19)) },
                new Account { Period = new DateRange(start, new DateTime(2026, 3, 15)) },
                new Account { Period = new DateRange(new DateTime(2026, 3, 15), end) },
                new Account { Period = new DateRange(new DateTime(2026, 3, 1), new DateTime(2026, 3, 9)) },
                new Account { Period = new DateRange(new DateTime(2026, 3, 21), new DateTime(2026, 3, 30)) },
                new Account { Period = new DateRange(new DateTime(2026, 3, 5), new DateTime(2026, 3, 25)) });
            await context.SaveChangesAsync(CancellationToken.None);

            var repository = new AccountingRepository(context);
            var result = await repository.GetByDateRangeAsync(start, end, CancellationToken.None);

            Assert.Equal(3, result.Count);
            Assert.All(result, a =>
            {
                Assert.True(a.Period.Start >= start && a.Period.End <= end);
            });
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task GetByDateRangeAsync_ReturnsEmpty_WhenNoAccountsInRange()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName(DatabasePrefix);
        using var context = LocalDbTestDatabase.CreateCreatedContext(databaseName);
        try
        {
            var repository = new AccountingRepository(context);
            var result = await repository.GetByDateRangeAsync(
                new DateTime(2026, 3, 10), new DateTime(2026, 3, 20), CancellationToken.None);

            Assert.Empty(result);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}
