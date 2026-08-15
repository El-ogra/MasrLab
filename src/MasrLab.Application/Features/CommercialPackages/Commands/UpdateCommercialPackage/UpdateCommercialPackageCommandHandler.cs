using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Commands.UpdateCommercialPackage;

public class UpdateCommercialPackageCommandHandler : IRequestHandler<UpdateCommercialPackageCommand, Unit>
{
    private readonly IRepository<CommercialPackage> _packageRepository;
    private readonly IRepository<CommercialPackageItem> _itemRepository;
    private readonly IRepository<CommercialPackagePrice> _priceRepository;
    private readonly IRepository<Test> _testRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommercialPackageCommandHandler(
        IRepository<CommercialPackage> packageRepository,
        IRepository<CommercialPackageItem> itemRepository,
        IRepository<CommercialPackagePrice> priceRepository,
        IRepository<Test> testRepository,
        IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _itemRepository = itemRepository;
        _priceRepository = priceRepository;
        _testRepository = testRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCommercialPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CommercialPackage), request.Id);

        package.Name = request.Name;
        _packageRepository.Update(package);

        var existingItems = await _itemRepository.GetAllAsync(cancellationToken);
        foreach (var item in existingItems.Where(i => i.CommercialPackageId == request.Id))
            _itemRepository.Delete(item);

        var existingPrices = await _priceRepository.GetAllAsync(cancellationToken);
        foreach (var price in existingPrices.Where(p => p.CommercialPackageId == request.Id))
            _priceRepository.Delete(price);

        var testIds = ParseInts(request.TestIds);
        for (int i = 0; i < testIds.Count; i++)
        {
            await _itemRepository.AddAsync(new CommercialPackageItem
            {
                CommercialPackageId = request.Id,
                TestId = testIds[i],
                DisplayOrder = i + 1
            }, cancellationToken);
        }

        var prices = ParsePrices(request.Prices);
        foreach (var (priceListId, price) in prices)
        {
            await _priceRepository.AddAsync(new CommercialPackagePrice
            {
                CommercialPackageId = request.Id,
                PriceListId = priceListId,
                Price = price
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
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
