using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;

public record DeletePriceListCommand(int PriceListId) : IRequest<Unit>;
