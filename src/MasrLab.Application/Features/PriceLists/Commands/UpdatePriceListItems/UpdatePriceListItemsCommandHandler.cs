using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItems;

public class UpdatePriceListItemsCommandHandler : IRequestHandler<UpdatePriceListItemsCommand, Unit>
{
    private readonly IRepository<PriceList> _priceListRepository;
    private readonly IRepository<PriceListItem> _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePriceListItemsCommandHandler(
        IRepository<PriceList> priceListRepository,
        IRepository<PriceListItem> itemRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdatePriceListItemsCommand request, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(request.PriceListId);
        if (priceList is null)
            throw new InvalidOperationException($"PriceList with Id {request.PriceListId} not found.");

        if (priceList.PriceListItems?.Any() == true)
        {
            foreach (var existingItem in priceList.PriceListItems.ToList())
            {
                _itemRepository.Delete(existingItem);
            }
        }

        foreach (var itemData in request.Items)
        {
            var newItem = new PriceListItem
            {
                PriceListId = request.PriceListId,
                TestId = itemData.TestId,
                Price = itemData.Price
            };
            await _itemRepository.AddAsync(newItem);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
