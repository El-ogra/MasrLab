using MediatR;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Features.Cultures.Commands.AddAntibioticToCulture;

public class AddAntibioticToCultureCommandHandler : IRequestHandler<AddAntibioticToCultureCommand, Unit>
{
    private readonly ICultureSensitivityService _cultureSensitivityService;

    public AddAntibioticToCultureCommandHandler(ICultureSensitivityService cultureSensitivityService)
    {
        _cultureSensitivityService = cultureSensitivityService;
    }

    public async Task<Unit> Handle(AddAntibioticToCultureCommand request, CancellationToken cancellationToken)
    {
        await _cultureSensitivityService.RecordSensitivityAsync(
            request.CultureId,
            request.AntibioticId,
            (int)request.SensitivityLevel,
            cancellationToken);

        return Unit.Value;
    }
}
