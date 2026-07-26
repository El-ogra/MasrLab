using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.EnterCultureResult;

public record EnterCultureResultCommand : IRequest<Unit>;
