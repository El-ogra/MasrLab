using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

public sealed class VisitAccountReader : IVisitAccountReader
{
    private readonly MasrLabDbContext _context;

    public VisitAccountReader(MasrLabDbContext context) => _context = context;

    public async Task<VisitAccountDto?> GetAsync(int patientVisitId, CancellationToken ct = default)
    {
        // The current account is the latest live receipt of the visit (global soft-delete
        // filter applies to receipts; their transactions are loaded explicitly below).
        var receipt = await _context.Receipts.AsNoTracking()
            .Where(r => r.PatientVisitId == patientVisitId)
            .OrderByDescending(r => r.Id)
            .Select(r => new
            {
                r.Id,
                r.PatientVisitId,
                r.DiscountPercent,
                r.Discount,
                r.Total,
                r.PaidPrevious,
                r.PaidNow,
                r.SettledAt
            })
            .FirstOrDefaultAsync(ct);
        if (receipt is null)
            return null;

        var testsTotal = await _context.VisitTests.AsNoTracking()
            .Where(vt => vt.ReceiptId == receipt.Id && !vt.IsDeleted)
            .SumAsync(vt => (decimal?)vt.Price, ct) ?? 0m;
        var extrasTotal = await _context.ExtraServiceItems.AsNoTracking()
            .Where(e => e.ReceiptId == receipt.Id && !e.IsDeleted)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;

        var transactions = await _context.Set<Domain.Entities.Financial.VisitPaymentTransaction>().AsNoTracking()
            .IgnoreQueryFilters() // deleted rows stay visible in the audit grid (flagged IsDeleted).
            .Where(t => t.ReceiptId == receipt.Id)
            .OrderBy(t => t.Id)
            .Select(t => new VisitAccountTransactionSource(
                t.Id, t.PaidDate, t.Amount, t.UserId, t.EditDate, t.Type, t.IsDeleted))
            .ToListAsync(ct);

        var userIds = transactions.Select(t => t.UserId).Distinct().ToList();
        var userNames = await _context.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username, ct);

        return VisitAccountDto.Create(
            receipt.Id,
            receipt.PatientVisitId,
            testsTotal + extrasTotal,
            receipt.DiscountPercent,
            receipt.Discount,
            receipt.Total,
            receipt.PaidPrevious,
            receipt.PaidNow,
            receipt.SettledAt is not null,
            transactions,
            userNames);
    }
}
