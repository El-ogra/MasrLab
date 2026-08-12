using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddReferenceValue;

public record AddReferenceValueCommand(
    int TestId,
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
