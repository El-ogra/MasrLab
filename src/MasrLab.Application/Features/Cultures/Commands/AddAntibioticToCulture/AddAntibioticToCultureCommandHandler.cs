using MediatR;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Cultures.Commands.AddAntibioticToCulture;

public class AddAntibioticToCultureCommandHandler : IRequestHandler<AddAntibioticToCultureCommand, Unit>
{
    private readonly ICultureRepository _cultureRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddAntibioticToCultureCommandHandler(ICultureRepository cultureRepository, IUnitOfWork unitOfWork)
    {
        _cultureRepository = cultureRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddAntibioticToCultureCommand request, CancellationToken cancellationToken)
    {
        var culture = await _cultureRepository.GetWithSensitivitiesAsync(request.CultureId);
        if (culture is null)
            throw new InvalidOperationException($"Culture with Id {request.CultureId} not found.");

        culture.RecordSensitivity(request.AntibioticId, request.SensitivityLevel);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
