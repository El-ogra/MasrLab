using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddTestComponent;

public record AddTestComponentCommand(
    int TestId,
    string Name,
    string Unit,
    int DisplayOrder,
    ResultEntryKind ResultEntryKind
) : IRequest<int>;
