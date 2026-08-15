using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Commands.AddCommercialPackage;

public class AddCommercialPackageCommandHandler : IRequestHandler<AddCommercialPackageCommand, int>
{
    private readonly IRepository<CommercialPackage> _packageRepository;
    private readonly IRepository<Test> _testRepository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCommercialPackageCommandHandler(
        IRepository<CommercialPackage> packageRepository,
        IRepository<Test> testRepository,
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _testRepository = testRepository;
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddCommercialPackageCommand request, CancellationToken cancellationToken)
    {
        var testIds = ParseInts(request.TestIds);
        if (testIds.Count == 0)
            throw new BusinessRuleViolationException("At least one test is required.");

        if (testIds.Distinct().Count() != testIds.Count)
            throw new BusinessRuleViolationException("Duplicate tests are not allowed in a package.");

        var allTests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = allTests.Where(t => testIds.Contains(t.Id)).ToDictionary(t => t.Id);
        var missingIds = testIds.Where(id => !testMap.ContainsKey(id)).ToList();
        if (missingIds.Count > 0)
            throw new BusinessRuleViolationException($"Tests not found: {string.Join(", ", missingIds)}");

        var prices = ParsePrices(request.Prices);
        if (prices.Count == 0)
            throw new BusinessRuleViolationException("At least one price entry is required.");

        var priceListIds = prices.Select(p => p.PriceListId).Distinct().ToList();
        var allPriceLists = await _priceListRepository.GetAllAsync(cancellationToken);
        var priceListMap = allPriceLists.ToDictionary(pl => pl.Id);
        var invalidPriceListIds = priceListIds.Where(id => !priceListMap.ContainsKey(id)).ToList();
        if (invalidPriceListIds.Count > 0)
            throw new BusinessRuleViolationException($"Price lists not found: {string.Join(", ", invalidPriceListIds)}");

        var defaultPriceList = await _priceListRepository.GetDefaultAsync(cancellationToken);
        if (defaultPriceList is not null && !prices.Any(p => p.PriceListId == defaultPriceList.Id))
            throw new BusinessRuleViolationException(
                $"A price for the active price list '{defaultPriceList.Name}' is required.");

        var package = new CommercialPackage
        {
            Name = request.Name,
            IsActive = true
        };

        for (int i = 0; i < testIds.Count; i++)
        {
            package.Items.Add(new CommercialPackageItem
            {
                TestId = testIds[i],
                DisplayOrder = i + 1
            });
        }

        foreach (var (priceListId, price) in prices)
        {
            package.Prices.Add(new CommercialPackagePrice
            {
                PriceListId = priceListId,
                Price = price
            });
        }

        await _packageRepository.AddAsync(package, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return package.Id;
    }

    private static List<int> ParseInts(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse).ToList();
    }

    private static List<(int PriceListId, decimal Price)> ParsePrices(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p =>
            {
                var parts = p.Split(':', StringSplitOptions.TrimEntries);
                return (int.Parse(parts[0]), decimal.Parse(parts[1]));
            }).ToList();
    }
}
