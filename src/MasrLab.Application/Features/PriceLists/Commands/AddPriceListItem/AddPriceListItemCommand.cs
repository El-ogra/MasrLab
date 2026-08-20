using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.AddPriceListItem;

public record AddPriceListItemCommand(int PriceListId, int TestId, decimal Price) : IRequest<int>;
