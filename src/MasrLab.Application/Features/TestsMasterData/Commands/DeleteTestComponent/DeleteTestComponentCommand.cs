using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.DeleteTestComponent;

public record DeleteTestComponentCommand(int Id, int TestId) : IRequest<Unit>;
