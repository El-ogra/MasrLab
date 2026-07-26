using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItems;

public record UpdatePriceListItemsCommand : IRequest<Unit>;
