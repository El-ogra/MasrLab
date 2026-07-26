using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;

public record CreatePriceListCommand(string Name) : IRequest<Unit>;
