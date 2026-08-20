using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceListItem;

public record DeletePriceListItemCommand(int PriceListItemId) : IRequest<Unit>;
