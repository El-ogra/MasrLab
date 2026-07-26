using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;

public record CreatePriceListCommand : IRequest<Unit>;
