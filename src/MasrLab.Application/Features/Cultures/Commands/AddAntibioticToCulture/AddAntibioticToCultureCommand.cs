using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.Cultures.Commands.AddAntibioticToCulture;

public record AddAntibioticToCultureCommand(
    int CultureId,
    int AntibioticId,
    SensitivityLevel SensitivityLevel
) : IRequest<Unit>;
