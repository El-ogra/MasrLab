using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;

public class GetPriceListForPrintQueryHandler : IRequestHandler<GetPriceListForPrintQuery, PriceListPrintDto?>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IRepository<Test> _testRepository;
    private readonly IMapper _mapper;

    public GetPriceListForPrintQueryHandler(
        IRepository<PriceList> repository,
        IRepository<Test> testRepository,
        IMapper mapper)
    {
        _repository = repository;
        _testRepository = testRepository;
        _mapper = mapper;
    }

    public async Task<PriceListPrintDto?> Handle(GetPriceListForPrintQuery request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            throw new EntityNotFoundException(nameof(PriceList), request.PriceListId);

        var allTests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = allTests.ToDictionary(t => t.Id);

        var items = priceList.PriceListItems?.Select(i =>
        {
            var dto = _mapper.Map<PriceListItemDto>(i);
            if (testMap.TryGetValue(i.TestId, out var test))
            {
                dto = dto with { TestGroupName = test.Group };
            }
            return dto;
        }).ToList() ?? new List<PriceListItemDto>();

        return new PriceListPrintDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            Items = items
        };
    }
}
