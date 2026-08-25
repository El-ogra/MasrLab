using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 4 — VisitAccountReader over a seeded receipt with payment + refund + extra rows.
public class VisitAccountReaderIntegrationTests
{
    [LocalDbFact]
    public async Task Reader_returns_figures_and_full_color_coded_transaction_grid()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M2Slice4Acct");

        int visitId = 1;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "receptionist", Password = "pwd", IsActive = true };
            setup.Users.Add(user);
            await setup.SaveChangesAsync(CancellationToken.None);

            var receipt = new Receipt
            {
                PatientVisitId = visitId,
                IssueDate = DateTime.UtcNow,
                ReceiveTime = DateTime.UtcNow,
                CreatedByUserId = user.Id
            };
            receipt.AddVisitTest(new VisitTest(visitId, 1, 100m, false)
            {
                TestNameSnapshot = "CBC",
                ReportNameSnapshot = "CBC",
                ReceiptNameSnapshot = "CBC"
            });
            receipt.Issue();
            setup.Receipts.Add(receipt);
            await setup.SaveChangesAsync(CancellationToken.None);

            receipt.RecordPayment(100m, user.Id);
            receipt.RecordRefund(20m, user.Id);
            receipt.RecordExtraCharge("Home visit", 50m, user.Id);
            await setup.SaveChangesAsync(CancellationToken.None);
        }

        await using var verificationContext = database.CreateContext();
        var reader = new VisitAccountReader(verificationContext);
        var account = await reader.GetAsync(visitId, CancellationToken.None);

        Assert.NotNull(account!);
        Assert.Equal(150m, account.TestsTotal); // tests 100 + extra 50.
        Assert.Equal(70m, account.RemainingForLab);   // total 150 − paid (100−20).
        Assert.Equal(0m, account.RemainingForPatient);
        Assert.Equal(3, account.Transactions.Count);

        var paymentRow = account.Transactions.Single(t => t.Type == Domain.Common.Enums.VisitTransactionType.Payment);
        Assert.Equal("Green", paymentRow.ColorCode);
        Assert.Equal("receptionist", paymentRow.UserName);

        Assert.Equal("Red", account.Transactions.Single(t => t.Type == Domain.Common.Enums.VisitTransactionType.Refund).ColorCode);
        Assert.Equal("Blue", account.Transactions.Single(t => t.Type == Domain.Common.Enums.VisitTransactionType.ExtraCharge).ColorCode);
        Assert.False(account.IsSettled);
    }
}
