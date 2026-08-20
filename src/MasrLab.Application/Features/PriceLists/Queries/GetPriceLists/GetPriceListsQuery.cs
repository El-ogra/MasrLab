using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceLists;

public record GetPriceListsQuery : IRequest<IReadOnlyList<PriceListDto>>;
