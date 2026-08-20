using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItem;

public record UpdatePriceListItemCommand(int PriceListItemId, decimal Price) : IRequest<Unit>;
