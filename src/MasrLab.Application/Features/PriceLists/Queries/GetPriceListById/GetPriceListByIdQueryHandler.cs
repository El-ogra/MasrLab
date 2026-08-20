using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;

public class GetPriceListByIdQueryHandler : IRequestHandler<GetPriceListByIdQuery, PriceListDto?>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IMapper _mapper;

    public GetPriceListByIdQueryHandler(IRepository<PriceList> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PriceListDto?> Handle(GetPriceListByIdQuery request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (priceList is null)
            return null;

        return _mapper.Map<PriceListDto>(priceList);
    }
}
