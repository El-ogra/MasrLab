using MasrLab.Application.Common.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetConsolidatedReport;

// M4-BR-13 print lines: group sub-titles, reference ranges and H/L flags.
// OQ-M4-14 binding placeholder for un-entered tests lives in Value.
public sealed record ConsolidatedReportLineDto(
    string TestName,
    string Value,
    string Unit,
    string ReferenceRange,
    string Flag)
{
    public const string NotEnteredPlaceholder = "لم يُدخل بعد";

    public static ConsolidatedReportLineDto Subtitle(string groupTitle) =>
        new(groupTitle, string.Empty, string.Empty, string.Empty, "SUBTITLE");

    public static ConsolidatedReportLineDto NotEntered(string testName) =>
        new(testName, NotEnteredPlaceholder, string.Empty, string.Empty, string.Empty);
}

public sealed record ConsolidatedReportDto(
    int ConsolidatedReportId,
    int PatientVisitId,
    string PatientName,
    string LaboratoryNumber,
    DateTime VisitDate,
    bool PrintGroupSubtitles,
    string? Comment,
    IReadOnlyList<ConsolidatedReportLineDto> Lines);

public sealed record GetConsolidatedReportQuery(int ConsolidatedReportId) : IRequest<ConsolidatedReportDto>;

public class GetConsolidatedReportQueryHandler : IRequestHandler<GetConsolidatedReportQuery, ConsolidatedReportDto>
{
    private readonly IClinicalReportReader _reader;

    public GetConsolidatedReportQueryHandler(IClinicalReportReader reader) => _reader = reader;

    public async Task<ConsolidatedReportDto> Handle(GetConsolidatedReportQuery request, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.ConsolidatedReportId);
        return await _reader.GetConsolidatedAsync(request.ConsolidatedReportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Consolidated report {request.ConsolidatedReportId} was not found.");
    }
}
