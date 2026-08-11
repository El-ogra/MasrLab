using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.DeleteTest;

public record DeleteTestCommand(int Id) : IRequest<Unit>;
