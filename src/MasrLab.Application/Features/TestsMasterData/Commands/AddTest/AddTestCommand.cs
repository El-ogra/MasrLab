using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddTest;

public record AddTestCommand : IRequest<Unit>;
