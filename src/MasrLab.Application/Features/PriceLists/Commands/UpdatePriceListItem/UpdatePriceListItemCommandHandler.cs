using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItem;

public class UpdatePriceListItemCommandHandler : IRequestHandler<UpdatePriceListItemCommand, Unit>
{
    private readonly IRepository<PriceListItem> _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePriceListItemCommandHandler(
        IRepository<PriceListItem> itemRepository,
        IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdatePriceListItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(request.PriceListItemId, cancellationToken);
        if (item is null)
            throw new EntityNotFoundException(nameof(PriceListItem), request.PriceListItemId);

        item.Price = request.Price;
        _itemRepository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
