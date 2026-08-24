using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.DeleteCultureAntibiotic;

public record DeleteCultureAntibioticCommand(int Id) : IRequest<Unit>;
