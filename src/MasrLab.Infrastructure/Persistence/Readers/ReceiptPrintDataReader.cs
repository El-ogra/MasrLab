using MasrLab.Application.Common.Printing;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

public sealed class ReceiptPrintDataReader : IReceiptPrintDataReader
{
    private readonly MasrLabDbContext _context;

    public ReceiptPrintDataReader(MasrLabDbContext context) => _context = context;

    public async Task<ReceiptPrintDto?> GetAsync(int receiptId, CancellationToken ct = default)
    {
        var receipt = await _context.Receipts.AsNoTracking()
            .Where(receipt => receipt.Id == receiptId)
            .Select(receipt => new
            {
                receipt.Id, receipt.PatientVisitId, receipt.IssueDate, receipt.Discount,
                receipt.DiscountPercent, receipt.Total, receipt.PaidPrevious, receipt.PaidNow,
                receipt.Remaining, receipt.Currency
            })
            .SingleOrDefaultAsync(ct);

        if (receipt is null)
            return null;

        var visit = await _context.PatientVisits.AsNoTracking()
            .Where(visit => visit.Id == receipt.PatientVisitId)
            .Select(visit => new { visit.PatientId, visit.LabId })
            .SingleOrDefaultAsync(ct);
        if (visit is null)
            return null;

        var patient = await _context.Patients.AsNoTracking()
            .Where(patient => patient.Id == visit.PatientId)
            .Select(patient => new { patient.Name, Phone = patient.Phone == null ? null : patient.Phone.Value })
            .SingleOrDefaultAsync(ct);
        if (patient is null)
            return null;

        var testLines = await _context.VisitTests.AsNoTracking()
            .Where(visitTest => visitTest.ReceiptId == receipt.Id)
            .Select(visitTest => new ReceiptPrintLineDto(
                visitTest.ReceiptNameSnapshot ?? visitTest.TestNameSnapshot,
                visitTest.Price))
            .ToListAsync(ct);

        var extraLines = await _context.ExtraServiceItems.AsNoTracking()
            .Where(item => item.ReceiptId == receipt.Id)
            .Select(item => new ReceiptPrintLineDto(item.Description, item.Amount))
            .ToListAsync(ct);

        var lines = testLines.Concat(extraLines).ToList();
        return new ReceiptPrintDto
        {
            ReceiptId = receipt.Id,
            ReceiptNumber = $"REC-{receipt.Id:D6}",
            IssueDate = receipt.IssueDate,
            PatientName = patient.Name,
            PatientLabId = visit.LabId,
            PatientPhone = patient.Phone,
            Lines = lines,
            GrossTotal = lines.Sum(line => line.Amount),
            Discount = receipt.Discount,
            DiscountPercent = receipt.DiscountPercent,
            Total = receipt.Total,
            PaidPrevious = receipt.PaidPrevious,
            PaidNow = receipt.PaidNow,
            Remaining = receipt.Remaining,
            RemainingForLab = Math.Max(0, receipt.Total - receipt.PaidPrevious - receipt.PaidNow),
            RemainingForPatient = Math.Max(0, receipt.PaidPrevious + receipt.PaidNow - receipt.Total),
            Currency = receipt.Currency
        };
    }
}
