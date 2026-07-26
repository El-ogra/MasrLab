using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.AddNewCulture;

public record AddNewCultureCommand : IRequest<Unit>;
