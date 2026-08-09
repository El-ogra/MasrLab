using MediatR;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Cultures.Commands.EnterCultureResult;

public class EnterCultureResultCommandHandler : IRequestHandler<EnterCultureResultCommand, Unit>
{
    private readonly ICultureRepository _cultureRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnterCultureResultCommandHandler(ICultureRepository cultureRepository, IUnitOfWork unitOfWork)
    {
        _cultureRepository = cultureRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(EnterCultureResultCommand request, CancellationToken cancellationToken)
    {
        var culture = await _cultureRepository.GetWithSensitivitiesAsync(request.CultureId, cancellationToken);
        if (culture is null)
            throw new EntityNotFoundException(nameof(Culture), request.CultureId);

        culture.Record(
            request.ColonyCount,
            request.OrganismA,
            request.OrganismB,
            request.OrganismC);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
