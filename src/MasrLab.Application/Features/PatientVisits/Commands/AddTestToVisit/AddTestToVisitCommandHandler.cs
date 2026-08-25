using MasrLab.Domain.Entities.Core;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.AddTestToVisit;

public class AddTestToVisitCommandHandler : IRequestHandler<AddTestToVisitCommand, Unit>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IPriceListResolverService _priceListResolverService;
    private readonly ITestRepository _testRepository;
    private readonly IVisitTestSnapshotter _snapshotter;
    private readonly IRepository<Sample> _sampleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddTestToVisitCommandHandler(
        IVisitRepository visitRepository,
        IPriceListRepository priceListRepository,
        IPriceListResolverService priceListResolverService,
        ITestRepository testRepository,
        IVisitTestSnapshotter snapshotter,
        IRepository<Sample> sampleRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _visitRepository = visitRepository;
        _priceListRepository = priceListRepository;
        _priceListResolverService = priceListResolverService;
        _testRepository = testRepository;
        _snapshotter = snapshotter;
        _sampleRepository = sampleRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(AddTestToVisitCommand request, CancellationToken cancellationToken)
    {
        _ = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated.");

        var visit = await _visitRepository.GetByIdAsync(request.PatientVisitId, cancellationToken);
        if (visit is null)
            throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        var priceListId = await ResolvePriceListIdAsync(request.PriceListId, cancellationToken);

        foreach (var testId in request.TestIds)
        {
            var test = await _testRepository.GetByIdWithComponentsAsync(testId, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Test), testId);

            var price = await _priceListResolverService.ResolvePriceAsync(testId, priceListId, cancellationToken);

            var (visitTest, resultItems) = _snapshotter.CreateVisitTestSnapshot(
                test, visit.Id, price, request.MarkOutsourced);

            visit.AddVisitTest(visitTest);
            visit.ExtendPromisedDelivery(test.TestTimeDays);

            var sample = Sample.Create(visit.Id, testId);
            await _sampleRepository.AddAsync(sample, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task<int> ResolvePriceListIdAsync(int? requestedPriceListId, CancellationToken cancellationToken)
    {
        if (requestedPriceListId.HasValue)
            return requestedPriceListId.Value;

        var defaultPriceList = await _priceListRepository.GetDefaultAsync(cancellationToken);
        if (defaultPriceList is null)
            throw new BusinessRuleViolationException(
                "No default price list configured and no price list specified. " +
                "Please set a default price list in system settings or provide a price list ID.");

        return defaultPriceList.Id;
    }
}
