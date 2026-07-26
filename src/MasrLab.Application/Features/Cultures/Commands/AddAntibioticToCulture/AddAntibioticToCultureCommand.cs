using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.AddAntibioticToCulture;

public record AddAntibioticToCultureCommand : IRequest<Unit>;
