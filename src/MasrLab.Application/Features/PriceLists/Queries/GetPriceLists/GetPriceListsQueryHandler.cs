using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceLists;

public class GetPriceListsQueryHandler : IRequestHandler<GetPriceListsQuery, IReadOnlyList<PriceListDto>>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IMapper _mapper;

    public GetPriceListsQueryHandler(IRepository<PriceList> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PriceListDto>> Handle(GetPriceListsQuery request, CancellationToken cancellationToken)
    {
        var priceLists = await _repository.GetAllAsync(cancellationToken);

        return priceLists.Select(pl => _mapper.Map<PriceListDto>(pl)).ToList();
    }
}
