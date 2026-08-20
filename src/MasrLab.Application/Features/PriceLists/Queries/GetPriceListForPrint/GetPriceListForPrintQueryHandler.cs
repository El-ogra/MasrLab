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
    private readonly IPriceListRepository _priceListRepository;
    private readonly IRepository<Test> _testRepository;
    private readonly IMapper _mapper;

    public GetPriceListForPrintQueryHandler(
        IPriceListRepository priceListRepository,
        IRepository<Test> testRepository,
        IMapper mapper)
    {
        _priceListRepository = priceListRepository;
        _testRepository = testRepository;
        _mapper = mapper;
    }

    public async Task<PriceListPrintDto?> Handle(GetPriceListForPrintQuery request, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdWithItemsAsync(request.PriceListId, cancellationToken);
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

        var categories = priceList.PriceListItems?
            .Where(i => testMap.ContainsKey(i.TestId))
            .GroupBy(i => testMap[i.TestId].Group)
            .OrderBy(g => g.Key)
            .Select(g => new PriceListPrintCategoryDto
            {
                ClinicalGroup = g.Key,
                Items = g.Select(i =>
                {
                    var test = testMap[i.TestId];
                    return new PriceListPrintItemDto
                    {
                        TestId = i.TestId,
                        TestName = test.Name,
                        Price = i.Price,
                        TurnaroundTime = test.TurnaroundTime,
                        CollectionNotes = test.SampleType
                    };
                }).ToList()
            }).ToList() ?? new List<PriceListPrintCategoryDto>();

        return new PriceListPrintDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            Currency = "L.E.",
            Categories = categories,
            Items = items
        };
    }
}
