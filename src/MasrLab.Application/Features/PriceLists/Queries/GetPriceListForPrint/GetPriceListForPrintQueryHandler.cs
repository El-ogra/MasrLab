using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;

public class GetPriceListForPrintQueryHandler : IRequestHandler<GetPriceListForPrintQuery, PriceListPrintDto?>
{
    private readonly IRepository<PriceList> _repository;

    public GetPriceListForPrintQueryHandler(IRepository<PriceList> repository)
    {
        _repository = repository;
    }

    public async Task<PriceListPrintDto?> Handle(GetPriceListForPrintQuery request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            throw new InvalidOperationException($"PriceList with Id {request.PriceListId} not found.");

        var items = priceList.PriceListItems?.Select(i => new PriceListItemDto
        {
            Id = i.Id,
            PriceListId = i.PriceListId,
            TestId = i.TestId,
            Price = i.Price
        }).ToList() ?? new List<PriceListItemDto>();

        return new PriceListPrintDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            Items = items
        };
    }
}
