using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;

public class GetPriceListForPrintQueryHandler : IRequestHandler<GetPriceListForPrintQuery, PriceListPrintDto?>
{
    public Task<PriceListPrintDto?> Handle(GetPriceListForPrintQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
