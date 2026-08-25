using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.Cultures.Commands.EnterCultureResult;

public record MicroscopicFindingInput(MicroscopicFindingRow RowKey, string Value);

public record EnterCultureResultCommand(
    int CultureId,
    string? OrganismA,
    string? OrganismB,
    string? OrganismC,
    string CultureCondition,
    int ColonyCount,
    string? SampleType = null,
    IReadOnlyList<MicroscopicFindingInput>? MicroscopicFindings = null
) : IRequest<Unit>;
