using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 1 — configuration round-trip, FK enforcement, and soft-delete filter
// for the per-visit payment transaction log (M2-BR-07).
public class VisitPaymentTransactionIntegrationTests
{
    private static Receipt CreateIssuedReceipt(int patientVisitId, int createdByUserId, decimal totalPrice)
    {
        var receipt = new Receipt
        {
            PatientVisitId = patientVisitId,
            IssueDate = DateTime.UtcNow,
            ReceiveTime = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
        receipt.AddVisitTest(new VisitTest(patientVisitId, 1, totalPrice, false)
        {
            TestNameSnapshot = "CBC",
            ReportNameSnapshot = "CBC",
            ReceiptNameSnapshot = "CBC"
        });
        receipt.Issue();
        return receipt;
    }

    [LocalDbFact]
    public async Task Transaction_rows_round_trip_through_the_real_schema()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M2Slice1TxLog");

        int receiptId;
        int paymentId;
        int refundId;
        int extraChargeId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "receptionist", Password = "pwd", IsAdmin = false, IsActive = true };
            setup.Users.Add(user);
            await setup.SaveChangesAsync(CancellationToken.None);

            var receipt = CreateIssuedReceipt(1, user.Id, 100m);
            setup.Receipts.Add(receipt);
            await setup.SaveChangesAsync(CancellationToken.None);

            receipt.RecordPayment(100m, user.Id);
            receipt.RecordRefund(20m, user.Id);
            receipt.RecordExtraCharge("Home visit", 50m, user.Id);
            await setup.SaveChangesAsync(CancellationToken.None);

            receiptId = receipt.Id;
            var rows = receipt.Transactions.OrderBy(t => t.Id).ToList();
            Assert.Equal(3, rows.Count);
            paymentId = rows[0].Id;
            refundId = rows[1].Id;
            extraChargeId = rows[2].Id;
        }

        await using (var verification = database.CreateContext())
        {
            var receipt = await verification.Receipts
                .Include(r => r.Transactions)
                .SingleAsync(r => r.Id == receiptId);
            Assert.Equal(3, receipt.Transactions.Count);
            Assert.Equal(80m, receipt.PaidNow);

            var payment = receipt.Transactions.Single(t => t.Id == paymentId);
            Assert.Equal(VisitTransactionType.Payment, payment.Type);
            Assert.Equal("Green", payment.ColorCode);
            Assert.Null(payment.EditDate);

            var refund = receipt.Transactions.Single(t => t.Id == refundId);
            Assert.Equal(VisitTransactionType.Refund, refund.Type);
            Assert.Equal("Red", refund.ColorCode);

            var extraCharge = receipt.Transactions.Single(t => t.Id == extraChargeId);
            Assert.Equal(VisitTransactionType.ExtraCharge, extraCharge.Type);
            Assert.Equal("Blue", extraCharge.ColorCode);
            Assert.Single(await verification.ExtraServiceItems.ToListAsync(), i => i.ReceiptId == receiptId && i.Amount == 50m);
        }
    }

    [LocalDbFact]
    public async Task Foreign_key_to_receipts_is_enforced()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M2Slice1TxFk");

        await using var context = database.CreateContext();
        var user = new User { Username = "u1", Password = "pwd", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        context.VisitPaymentTransactions.Add(VisitPaymentTransaction.Create(
            987654, VisitTransactionType.Payment, 10m, user.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken.None));
    }

    [LocalDbFact]
    public async Task Soft_deleted_transactions_are_hidden_by_the_global_filter_but_kept_in_table()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M2Slice1TxSoftDel");

        int transactionId;
        int userId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "u2", Password = "pwd", IsActive = true };
            setup.Users.Add(user);
            await setup.SaveChangesAsync(CancellationToken.None);

            var receipt = CreateIssuedReceipt(1, user.Id, 100m);
            setup.Receipts.Add(receipt);
            await setup.SaveChangesAsync(CancellationToken.None);

            receipt.RecordPayment(40m, user.Id);
            await setup.SaveChangesAsync(CancellationToken.None);
            transactionId = receipt.Transactions.Single().Id;
            userId = user.Id;
        }

        await using (var deleteContext = database.CreateContext())
        {
            var transaction = await deleteContext.VisitPaymentTransactions.SingleAsync(t => t.Id == transactionId);
            deleteContext.VisitPaymentTransactions.Remove(transaction);
            await deleteContext.SaveChangesAsync(CancellationToken.None);
        }

        await using var verification = database.CreateContext();
        Assert.False(await verification.VisitPaymentTransactions.AnyAsync(t => t.Id == transactionId));

        var archived = await verification.VisitPaymentTransactions
            .IgnoreQueryFilters()
            .SingleAsync(t => t.Id == transactionId);
        Assert.True(archived.IsDeleted);
        Assert.Equal(userId, archived.UserId);
    }
}
