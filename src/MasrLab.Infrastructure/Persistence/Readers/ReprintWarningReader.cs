using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.ResultsEntry.Queries.GetReprintWarning;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Readers;

public sealed class ReprintWarningReader : IReprintWarningReader
{
    private readonly MasrLabDbContext _context;

    public ReprintWarningReader(MasrLabDbContext context) => _context = context;

    public async Task<ReprintWarningDto> GetAsync(int visitTestId, CancellationToken cancellationToken = default)
    {
        // OQ-M4-7: latest print metadata across all entered results of the visit test.
        var lastPrint = await (
                from item in _context.VisitTestResultItems.AsNoTracking()
                join result in _context.TestResults.AsNoTracking()
                    on item.Id equals result.VisitTestResultItemId
                where item.VisitTestId == visitTestId
                      && result.PrintCount > 0
                      && result.PrintedAt != null
                orderby result.PrintedAt descending
                select new { PrintedAt = (DateTime?)result.PrintedAt, result.PrintedByUserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (lastPrint is not { PrintedAt: { } printedAt } || lastPrint.PrintedByUserId is not { } userId)
            return ReprintWarningDto.NotPrinted(visitTestId);

        var userName = await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(cancellationToken);

        return new ReprintWarningDto(
            visitTestId,
            HasBeenPrinted: true,
            LastPrintedAtUtc: printedAt,
            LastPrintedByUserName: userName ?? $"User {userId}",
            WarningMessage: ReprintWarningDto.BuildMessage(printedAt, userName ?? $"User {userId}"));
    }
}
