using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.RecordSensitivity;

// OQ-M4-12/13: per-organism four-category classification with an optional
// inhibition-zone override (default display falls back to M13 SensitivityText).
public sealed record RecordSensitivityCommand(
    int CultureId,
    OrganismSlot OrganismSlot,
    int AntibioticId,
    SensitivityLevel Level,
    string? InhibitionZoneOverride = null) : IRequest<int>;

public class RecordSensitivityCommandHandler : IRequestHandler<RecordSensitivityCommand, int>
{
    private readonly ICultureRepository _cultureRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordSensitivityCommandHandler(ICultureRepository cultureRepository, IUnitOfWork unitOfWork)
    {
        _cultureRepository = cultureRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(RecordSensitivityCommand request, CancellationToken cancellationToken)
    {
        var culture = await _cultureRepository.GetWithSensitivitiesAsync(request.CultureId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Culture), request.CultureId);

        culture.RecordSensitivity(
            request.OrganismSlot,
            request.AntibioticId,
            request.Level);

        if (!string.IsNullOrWhiteSpace(request.InhibitionZoneOverride))
        {
            var recorded = culture.Sensitivities
                .Last(s => s.OrganismSlot == request.OrganismSlot && s.AntibioticId == request.AntibioticId);
            recorded.SetInhibitionZoneOverride(request.InhibitionZoneOverride);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return culture.Sensitivities.Last().Id;
    }
}
