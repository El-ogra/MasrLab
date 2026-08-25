using MasrLab.Application.Common.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.Cultures.Queries.GetMicrobiologyReport;

// M4-BR-18/19/20: microscopic block + per-organism four-category sensitivity tables.
public sealed record MicrobiologyFindingRowDto(string RowKey, string Value, string ReferenceRange);

public sealed record SensitivityRowDto(
    int AntibioticId,
    string AntibioticName,
    string CommercialName,
    string Level,
    string InhibitionZone);

public sealed record OrganismSensitivityBlockDto(
    string OrganismLabel,
    IReadOnlyList<SensitivityRowDto> Rows);

public sealed record MicrobiologyReportDto(
    int CultureId,
    string SampleType,
    string CultureCondition,
    int ColonyCount,
    bool ShowSensitivityInReport,
    bool ShowReferenceInReport,
    bool ShowCommercialNameInReport,
    IReadOnlyList<MicrobiologyFindingRowDto> MicroscopicRows,
    IReadOnlyList<OrganismSensitivityBlockDto> OrganismBlocks);

public sealed record GetMicrobiologyReportQuery(int CultureId) : IRequest<MicrobiologyReportDto>;

public class GetMicrobiologyReportQueryHandler : IRequestHandler<GetMicrobiologyReportQuery, MicrobiologyReportDto>
{
    private readonly IMicrobiologyReportReader _reader;

    public GetMicrobiologyReportQueryHandler(IMicrobiologyReportReader reader) => _reader = reader;

    public async Task<MicrobiologyReportDto> Handle(GetMicrobiologyReportQuery request, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.CultureId);
        return await _reader.GetAsync(request.CultureId, cancellationToken)
            ?? throw new KeyNotFoundException($"Culture {request.CultureId} was not found.");
    }
}
