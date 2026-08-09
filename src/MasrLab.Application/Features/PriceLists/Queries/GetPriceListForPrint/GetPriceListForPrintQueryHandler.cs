using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;

public class GetPriceListForPrintQueryHandler : IRequestHandler<GetPriceListForPrintQuery, PriceListPrintDto?>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IMapper _mapper;

    public GetPriceListForPrintQueryHandler(IRepository<PriceList> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PriceListPrintDto?> Handle(GetPriceListForPrintQuery request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            throw new InvalidOperationException($"PriceList with Id {request.PriceListId} not found.");

        // PriceListPrintDto aggregates a PriceList with its items, so the outer object is
        // assembled manually; each inner PriceListItemDto is a simple map.
        var items = priceList.PriceListItems?.Select(i => _mapper.Map<PriceListItemDto>(i)).ToList()
            ?? new List<PriceListItemDto>();

        return new PriceListPrintDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            Items = items
        };
    }
}
