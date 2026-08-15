using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValue;

public record UpdateReferenceValueCommand(
    int Id,
    int TestId,
    int? TestComponentId,
    ReferenceValueGender Gender,
    int AgeMin,
    int AgeMax,
    AgeUnit AgeUnit,
    string NormalRange,
    decimal? LowLimit,
    decimal? HighLimit,
    string? TestUnit,
    string? LowFlag,
    string? HighFlag,
    bool ForPregnantOnly,
    string? HighComment,
    string? LowComment
) : IRequest<Unit>;
