using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.SetDefaultPriceList;

public record SetDefaultPriceListCommand(int PriceListId) : IRequest<Unit>;
