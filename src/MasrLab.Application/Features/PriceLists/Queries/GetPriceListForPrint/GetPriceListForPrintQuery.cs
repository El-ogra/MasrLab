using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;

public record GetPriceListForPrintQuery : IRequest<ReceiptDto?>;
