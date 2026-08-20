using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceListItem;

public class DeletePriceListItemCommandHandler : IRequestHandler<DeletePriceListItemCommand, Unit>
{
    private readonly IRepository<PriceListItem> _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePriceListItemCommandHandler(
        IRepository<PriceListItem> itemRepository,
        IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeletePriceListItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(request.PriceListItemId, cancellationToken);
        if (item is null)
            throw new EntityNotFoundException(nameof(PriceListItem), request.PriceListItemId);

        item.IsDeleted = true;
        _itemRepository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
