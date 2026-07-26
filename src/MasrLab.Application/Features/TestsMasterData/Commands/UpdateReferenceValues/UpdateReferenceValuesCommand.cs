using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValues;

public record UpdateReferenceValuesCommand(
    int TestId,
    Gender Gender,
    int AgeMin,
    int AgeMax,
    AgeUnit AgeUnit,
    string NormalRange,
    string? HighComment,
    string? LowComment
) : IRequest<Unit>;
