using MasrLab.Application.Common.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetBlankReport;

public sealed record BlankReportRowDto(
    int Id,
    string TestName,
    string Result,
    string Unit,
    string Flag,
    string ReferenceRange,
    int DisplayOrder);

public sealed record BlankReportDto(
    int BlankReportId,
    int PatientVisitId,
    string PatientName,
    string LaboratoryNumber,
    DateTime VisitDate,
    string ReportTitle,
    string? Comment,
    string PaginationNote,
    int PrintCount,
    IReadOnlyList<BlankReportRowDto> Rows);

public sealed record GetBlankReportQuery(int BlankReportId) : IRequest<BlankReportDto>;

public class GetBlankReportQueryHandler : IRequestHandler<GetBlankReportQuery, BlankReportDto>
{
    private readonly IBlankReportReader _reader;

    public GetBlankReportQueryHandler(IBlankReportReader reader) => _reader = reader;

    public async Task<BlankReportDto> Handle(GetBlankReportQuery request, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.BlankReportId);
        return await _reader.GetAsync(request.BlankReportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Blank report {request.BlankReportId} was not found.");
    }
}
