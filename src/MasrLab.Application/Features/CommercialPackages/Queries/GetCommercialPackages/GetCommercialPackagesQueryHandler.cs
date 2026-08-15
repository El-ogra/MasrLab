using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Queries.GetCommercialPackages;

public class GetCommercialPackagesQueryHandler : IRequestHandler<GetCommercialPackagesQuery, IReadOnlyList<CommercialPackageDto>>
{
    private readonly IRepository<CommercialPackage> _packageRepository;
    private readonly IRepository<Test> _testRepository;

    public GetCommercialPackagesQueryHandler(
        IRepository<CommercialPackage> packageRepository,
        IRepository<Test> testRepository)
    {
        _packageRepository = packageRepository;
        _testRepository = testRepository;
    }

    public async Task<IReadOnlyList<CommercialPackageDto>> Handle(GetCommercialPackagesQuery request, CancellationToken cancellationToken)
    {
        var packages = await _packageRepository.GetAllAsync(cancellationToken);
        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        return packages.Select(p => new CommercialPackageDto
        {
            Id = p.Id,
            Name = p.Name,
            IsActive = p.IsActive,
            Items = p.Items.Select(i => new CommercialPackageItemDto
            {
                Id = i.Id,
                TestId = i.TestId,
                TestName = testMap.TryGetValue(i.TestId, out var t) ? t.Name : string.Empty,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            Prices = p.Prices.Select(pr => new CommercialPackagePriceDto
            {
                Id = pr.Id,
                PriceListId = pr.PriceListId,
                Price = pr.Price
            }).ToList()
        }).ToList();
    }
}
