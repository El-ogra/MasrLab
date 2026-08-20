using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;

public class GetPriceListByIdQueryHandler : IRequestHandler<GetPriceListByIdQuery, PriceListWithItemsDto?>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IMapper _mapper;

    public GetPriceListByIdQueryHandler(IPriceListRepository priceListRepository, IMapper mapper)
    {
        _priceListRepository = priceListRepository;
        _mapper = mapper;
    }

    public async Task<PriceListWithItemsDto?> Handle(GetPriceListByIdQuery request, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdWithItemsAsync(request.Id, cancellationToken);

        if (priceList is null)
            return null;

        return _mapper.Map<PriceListWithItemsDto>(priceList);
    }
}
