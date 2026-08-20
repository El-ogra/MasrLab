using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MediatR;

namespace MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;

public class AddTestsToVisitCommandHandler : IRequestHandler<AddTestsToVisitCommand, Unit>
{
    private readonly IVisitRepository _visitRepository;
    private readonly ITestRepository _testRepository;
    private readonly IRepository<TestGroup> _groupRepository;
    private readonly ITestGroupItemRepository _groupItemRepository;
    private readonly ICommercialPackageRepository _packageRepository;
    private readonly IRepository<VisitCommercialPackage> _visitPackageRepository;
    private readonly IPriceListResolverService _priceListResolver;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IVisitTestSnapshotter _snapshotter;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestsToVisitCommandHandler(
        IVisitRepository visitRepository,
        ITestRepository testRepository,
        IRepository<TestGroup> groupRepository,
        ITestGroupItemRepository groupItemRepository,
        ICommercialPackageRepository packageRepository,
        IRepository<VisitCommercialPackage> visitPackageRepository,
        IPriceListResolverService priceListResolver,
        IPriceListRepository priceListRepository,
        IVisitTestSnapshotter snapshotter,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _testRepository = testRepository;
        _groupRepository = groupRepository;
        _groupItemRepository = groupItemRepository;
        _packageRepository = packageRepository;
        _visitPackageRepository = visitPackageRepository;
        _priceListResolver = priceListResolver;
        _priceListRepository = priceListRepository;
        _snapshotter = snapshotter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddTestsToVisitCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        var testIdsToProcess = await ResolveTestIdsAsync(request, cancellationToken);

        if (testIdsToProcess.Count == 0)
            throw new BusinessRuleViolationException("No tests to add.");

        if (testIdsToProcess.Count > 10 && !request.ConfirmLargeExpansion)
            throw new BusinessRuleViolationException(
                $"Expansion produces {testIdsToProcess.Count} tests. Set ConfirmLargeExpansion=true to proceed.");

        var existingTestIds = visit.VisitTests.Select(vt => vt.TestId).ToHashSet();
        var seenTestIds = new HashSet<int>();
        var duplicateTestIds = new List<int>();

        foreach (var testId in testIdsToProcess)
        {
            if (existingTestIds.Contains(testId) || !seenTestIds.Add(testId))
                duplicateTestIds.Add(testId);
        }

        if (duplicateTestIds.Count > 0)
            throw new BusinessRuleViolationException(
                $"Duplicate tests cannot be added: {string.Join(", ", duplicateTestIds.Distinct())}");

        var allTests = await _testRepository.GetAllWithComponentsAsync(cancellationToken);
        var testMap = allTests.Where(t => testIdsToProcess.Contains(t.Id)).ToDictionary(t => t.Id);

        var defaultPriceList = await _priceListRepository.GetDefaultAsync(cancellationToken);
        var priceListId = defaultPriceList?.Id ?? 0;

        // --- Slice 8: load group snapshot data for SelectionGroup source ---
        TestGroup? selectionGroup = null;
        Dictionary<int, decimal>? groupPriceMap = null;
        if (request.Source == "SelectionGroup" && request.TestGroupId.HasValue)
        {
            selectionGroup = await _groupRepository.GetByIdAsync(request.TestGroupId.Value, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(TestGroup), request.TestGroupId.Value);

            var groupItems = await _groupItemRepository.GetByTestGroupIdAsync(request.TestGroupId.Value, cancellationToken);
            groupPriceMap = groupItems.ToDictionary(i => i.TestId, i => i.Price);
        }

        VisitCommercialPackage? visitPackage = null;
        CommercialPackage? package = null;
        if (request.Source == "CommercialPackage" && request.CommercialPackageId.HasValue)
        {
            package = await _packageRepository.GetByIdWithItemsAndPricesAsync(request.CommercialPackageId.Value, cancellationToken);
            if (package is not null)
            {
                var packagePrice = priceListId > 0
                    ? package.Prices.FirstOrDefault(p => p.PriceListId == priceListId)?.Price ?? 0m
                    : 0m;

                visitPackage = new VisitCommercialPackage
                {
                    PatientVisitId = visit.Id,
                    CommercialPackageId = package.Id,
                    PackageNameSnapshot = package.Name,
                    Price = packagePrice
                };
                visit.CommercialPackages.Add(visitPackage);
            }
        }

        var newVisitTests = new List<VisitTest>();
        foreach (var testId in testIdsToProcess)
        {
            var test = testMap[testId];

            // Slice 8: SelectionGroup uses TestGroupItem.Price; other sources use price list.
            decimal price;
            if (request.Source == "SelectionGroup" && groupPriceMap is not null && groupPriceMap.TryGetValue(testId, out var groupPrice))
            {
                price = groupPrice;
            }
            else
            {
                price = priceListId > 0
                    ? await _priceListResolver.ResolvePriceAsync(testId, priceListId, cancellationToken)
                    : test.Price;
            }

            var (visitTest, resultItems) = request.Source == "SelectionGroup" && selectionGroup is not null
                ? _snapshotter.CreateVisitTestSnapshot(
                    test, visit.Id, price, isOutsourced: false,
                    sourceTestGroupId: selectionGroup.Id,
                    testGroupNameSnapshot: selectionGroup.GroupName)
                : _snapshotter.CreateVisitTestSnapshot(
                    test, visit.Id, price, isOutsourced: false);

            visit.AddVisitTest(visitTest);
            visit.ExtendPromisedDelivery(test.TestTimeDays);
            newVisitTests.Add(visitTest);

            var sample = Sample.Create(visit.Id, testId);
            visit.Samples.Add(sample);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (visitPackage is not null)
        {
            foreach (var vt in newVisitTests)
            {
                vt.VisitCommercialPackageId = visitPackage.Id;
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }

    private async Task<List<int>> ResolveTestIdsAsync(AddTestsToVisitCommand request, CancellationToken ct)
    {
        return request.Source switch
        {
            "Direct" => ParseInts(request.DirectTestIds!),
            "LegacyGroup" => await ResolveLegacyGroupTestsAsync(request.DirectTestIds!, ct),
            "SelectionGroup" => await ResolveSelectionGroupTestsAsync(request.TestGroupId!.Value, ct),
            "CommercialPackage" => await ResolveCommercialPackageTestsAsync(request.CommercialPackageId!.Value, ct),
            _ => throw new BusinessRuleViolationException($"Unknown source: {request.Source}")
        };
    }

    private async Task<List<int>> ResolveLegacyGroupTestsAsync(string testIds, CancellationToken ct)
    {
        var ids = ParseInts(testIds);
        if (ids.Count == 0) return new();

        var test = await _testRepository.GetByIdAsync(ids.First(), ct);
        if (test is null) return new();

        var allTests = await _testRepository.GetAllAsync(ct);
        return allTests.Where(t => t.Group == test.Group && !t.IsDeleted).Select(t => t.Id).ToList();
    }

    private async Task<List<int>> ResolveSelectionGroupTestsAsync(int groupId, CancellationToken ct)
    {
        var items = await _groupItemRepository.GetByTestGroupIdAsync(groupId, ct);
        return items.OrderBy(i => i.DisplayOrder).Select(i => i.TestId).ToList();
    }

    private async Task<List<int>> ResolveCommercialPackageTestsAsync(int packageId, CancellationToken ct)
    {
        var package = await _packageRepository.GetByIdWithItemsAndPricesAsync(packageId, ct);
        if (package is null) return new();
        return package.Items.OrderBy(i => i.DisplayOrder).Select(i => i.TestId).ToList();
    }

    private static List<int> ParseInts(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse).ToList();
    }
}
