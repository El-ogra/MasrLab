using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientVisits.Queries.GetVisitAccount;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

// Slice 4 — account-window DTO shaping, OQ-M2-6 color mapping, deleted-row visibility.
public class Module02Slice4VisitAccountTests
{
    private static readonly Dictionary<int, string> Users = new() { [7] = "receptionist", [9] = "manager" };

    private static VisitAccountTransactionSource Source(
        int id, VisitTransactionType type, decimal amount = 10m, bool isDeleted = false) =>
        new(id, new DateTime(2026, 8, 25, 10, 0, 0, DateTimeKind.Utc).AddMinutes(id), amount, 7,
            null, type, isDeleted);

    [Fact]
    public void Create_ShapesTheFiguresPanelPerM2_BR_05()
    {
        var dto = VisitAccountDto.Create(
            receiptId: 5, patientVisitId: 3,
            testsTotal: 184m + 16m, // tests + extra charges
            discountPercent: 10m, discountValue: 20m,
            totalAfterDiscount: 180m,
            previouslyPaid: 120m, paidNow: 60m,
            isSettled: false,
            transactions: Array.Empty<VisitAccountTransactionSource>(),
            userNamesById: Users);

        Assert.Equal(200m, dto.TestsTotal);
        Assert.Equal(10m, dto.DiscountPercent);
        Assert.Equal(20m, dto.DiscountValue);
        Assert.Equal(180m, dto.TotalAfterDiscount);
        Assert.Equal(120m, dto.PreviouslyPaid);
        Assert.Equal(0m, dto.RemainingForLab);   // paid total 180 == total.
        Assert.Equal(0m, dto.RemainingForPatient);
        Assert.False(dto.IsSettled);
    }

    [Theory]
    [InlineData(VisitTransactionType.Payment, "Green")]
    [InlineData(VisitTransactionType.Refund, "Red")]
    [InlineData(VisitTransactionType.ExtraCharge, "Blue")]
    [InlineData(VisitTransactionType.Adjustment, "Yellow")]
    public void Create_MapsColorCodesPerOQ_M2_6(VisitTransactionType type, string expectedColor)
    {
        var dto = VisitAccountDto.Create(
            1, 1, 100m, 0m, 0m, 100m, 0m, 0m, false,
            new[] { Source(1, type) }, Users);

        Assert.Equal(expectedColor, Assert.Single(dto.Transactions).ColorCode);
    }

    [Fact]
    public void Create_KeepsDeletedRowsVisible_FlaggedIsDeleted()
    {
        var dto = VisitAccountDto.Create(
            1, 1, 100m, 0m, 0m, 100m, 0m, 50m, true,
            new[]
            {
                Source(1, VisitTransactionType.Payment, 60m),
                Source(2, VisitTransactionType.Refund, 20m, isDeleted: true),
                Source(3, VisitTransactionType.Adjustment, 20m)
            },
            Users);

        Assert.Equal(3, dto.Transactions.Count); // audit grid shows every row...
        Assert.True(dto.Transactions[1].IsDeleted); // ...with deletion flagged for display.
        Assert.False(dto.Transactions[0].IsDeleted);
        Assert.Equal("receptionist", dto.Transactions[0].UserName);
        Assert.True(dto.IsSettled);
        // Deleted rows do not silently vanish from ordering — grid renders chronological log.
        Assert.True(dto.Transactions[0].PaidDate <= dto.Transactions[1].PaidDate);
        Assert.True(dto.Transactions[1].PaidDate <= dto.Transactions[2].PaidDate);
    }

    [Fact]
    public void Create_FallsBackToGenericUserNameForUnknownUser()
    {
        var dto = VisitAccountDto.Create(
            1, 1, 100m, 0m, 0m, 100m, 0m, 0m, false,
            new[] { Source(1, VisitTransactionType.Payment) },
            new Dictionary<int, string>()); // no users resolved

        Assert.Equal("User 7", Assert.Single(dto.Transactions).UserName);
    }

    [Fact]
    public async Task GetVisitAccountQuery_RejectsNonPositiveVisitId()
    {
        var handler = new GetVisitAccountQueryHandler(new MockReader(null));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.Handle(new GetVisitAccountQuery(0), default));
    }

    [Fact]
    public async Task GetVisitAccountQuery_PropagatesMissingAccountAsKeyNotFound()
    {
        var handler = new GetVisitAccountQueryHandler(new MockReader(null));
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetVisitAccountQuery(42), default));
    }

    private sealed class MockReader(VisitAccountDto? result) : IVisitAccountReader
    {
        public Task<VisitAccountDto?> GetAsync(int patientVisitId, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
