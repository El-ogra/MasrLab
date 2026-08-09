using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Commands.SetDefaultPriceList;

public class SetDefaultPriceListCommandHandler : IRequestHandler<SetDefaultPriceListCommand, Unit>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetDefaultPriceListCommandHandler(
        IRepository<PriceList> repository,
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetDefaultPriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            throw new EntityNotFoundException(nameof(PriceList), request.PriceListId);

        // Clear the current default if it's a different list
        var currentDefault = await _priceListRepository.GetDefaultAsync(cancellationToken);
        if (currentDefault is not null && currentDefault.Id != request.PriceListId)
        {
            currentDefault.IsDefault = false;
            _repository.Update(currentDefault);
        }

        priceList.IsDefault = true;
        _repository.Update(priceList);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
