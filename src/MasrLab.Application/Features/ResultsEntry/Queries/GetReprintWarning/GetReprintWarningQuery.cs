using MasrLab.Application.Common.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetReprintWarning;

public sealed record ReprintWarningDto(
    int VisitTestId,
    bool HasBeenPrinted,
    DateTime? LastPrintedAtUtc,
    string? LastPrintedByUserName,
    string? WarningMessage)
{
    public static ReprintWarningDto NotPrinted(int visitTestId) =>
        new(visitTestId, false, null, null, null);

    // OQ-M4-7 binding message: "This report was previously printed on [date/time] by
    // [user]. Do you want to continue?"
    public static string BuildMessage(DateTime printedAtUtc, string userName) =>
        $"This report was previously printed on {printedAtUtc:yyyy-MM-dd HH:mm} UTC by {userName}. Do you want to continue?";
}

public sealed record GetReprintWarningQuery(int VisitTestId) : IRequest<ReprintWarningDto>;

public class GetReprintWarningQueryHandler : IRequestHandler<GetReprintWarningQuery, ReprintWarningDto>
{
    private readonly IReprintWarningReader _reader;

    public GetReprintWarningQueryHandler(IReprintWarningReader reader) => _reader = reader;

    public Task<ReprintWarningDto> Handle(GetReprintWarningQuery request, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.VisitTestId);
        return _reader.GetAsync(request.VisitTestId, cancellationToken);
    }
}
