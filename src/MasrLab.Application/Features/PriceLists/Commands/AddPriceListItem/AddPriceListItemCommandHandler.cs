using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.AddPriceListItem;

public class AddPriceListItemCommandHandler : IRequestHandler<AddPriceListItemCommand, int>
{
    private readonly IRepository<PriceList> _priceListRepository;
    private readonly IRepository<Test> _testRepository;
    private readonly IPriceListItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPriceListItemCommandHandler(
        IRepository<PriceList> priceListRepository,
        IRepository<Test> testRepository,
        IPriceListItemRepository itemRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _testRepository = testRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddPriceListItemCommand request, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            throw new EntityNotFoundException(nameof(PriceList), request.PriceListId);

        var test = await _testRepository.GetByIdAsync(request.TestId, cancellationToken);
        if (test is null)
            throw new EntityNotFoundException(nameof(Test), request.TestId);

        var existing = await _itemRepository.GetByPriceListAndTestAsync(
            request.PriceListId, request.TestId, cancellationToken);
        if (existing is not null)
            throw new BusinessRuleViolationException(
                "A price-list item for this price list and test already exists.");

        var item = new PriceListItem
        {
            PriceListId = request.PriceListId,
            TestId = request.TestId,
            Price = request.Price
        };

        await _itemRepository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return item.Id;
    }
}
