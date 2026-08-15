using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Queries.GetCommercialPackageById;

public class GetCommercialPackageByIdQueryHandler : IRequestHandler<GetCommercialPackageByIdQuery, CommercialPackageDto?>
{
    private readonly IRepository<CommercialPackage> _packageRepository;
    private readonly IRepository<Test> _testRepository;

    public GetCommercialPackageByIdQueryHandler(
        IRepository<CommercialPackage> packageRepository,
        IRepository<Test> testRepository)
    {
        _packageRepository = packageRepository;
        _testRepository = testRepository;
    }

    public async Task<CommercialPackageDto?> Handle(GetCommercialPackageByIdQuery request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (package is null) return null;

        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        return new CommercialPackageDto
        {
            Id = package.Id,
            Name = package.Name,
            IsActive = package.IsActive,
            Items = package.Items.Select(i => new CommercialPackageItemDto
            {
                Id = i.Id,
                TestId = i.TestId,
                TestName = testMap.TryGetValue(i.TestId, out var t) ? t.Name : string.Empty,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            Prices = package.Prices.Select(pr => new CommercialPackagePriceDto
            {
                Id = pr.Id,
                PriceListId = pr.PriceListId,
                Price = pr.Price
            }).ToList()
        };
    }
}
