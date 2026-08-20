using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;

public record GetPriceListByIdQuery(int Id) : IRequest<PriceListDto?>;
