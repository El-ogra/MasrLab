using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.DeleteReferenceValue;

public record DeleteReferenceValueCommand(int Id) : IRequest<Unit>;
