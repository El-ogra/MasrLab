using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateTestComponent;

public record UpdateTestComponentCommand(
    int Id,
    int TestId,
    string Name,
    string Unit,
    int DisplayOrder,
    ResultEntryKind ResultEntryKind
) : IRequest<Unit>;
