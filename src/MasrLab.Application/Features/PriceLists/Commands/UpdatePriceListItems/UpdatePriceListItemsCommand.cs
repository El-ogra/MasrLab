using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItems;

public record UpdatePriceListItemsCommand(int PriceListId, List<PriceListItemData> Items) : IRequest<Unit>;

public record PriceListItemData(int TestId, decimal Price);
