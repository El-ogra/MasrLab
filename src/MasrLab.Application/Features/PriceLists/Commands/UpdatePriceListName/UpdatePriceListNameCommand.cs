using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;

public record UpdatePriceListNameCommand(int PriceListId, string Name) : IRequest<Unit>;
