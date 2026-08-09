using MasrLab.Domain.Entities.Core;
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
    private readonly IRepository<Sample> _sampleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestToVisitCommandHandler(
        IVisitRepository visitRepository,
        IPriceListRepository priceListRepository,
        IPriceListResolverService priceListResolverService,
        IRepository<Sample> sampleRepository,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _priceListRepository = priceListRepository;
        _priceListResolverService = priceListResolverService;
        _sampleRepository = sampleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddTestToVisitCommand request, CancellationToken cancellationToken)
    {
        // 1. Load the tracked visit
        var visit = await _visitRepository.GetByIdAsync(request.PatientVisitId, cancellationToken);
        if (visit is null)
            throw new InvalidOperationException($"PatientVisit with Id {request.PatientVisitId} not found.");

        // 2. Resolve the price list
        var priceListId = await ResolvePriceListIdAsync(request.PriceListId, cancellationToken);

        // 3. For each test: resolve price → add test → create sample
        foreach (var testId in request.TestIds)
        {
            var price = await _priceListResolverService.ResolvePriceAsync(testId, priceListId, cancellationToken);
            visit.AddTest(testId, price, request.MarkOutsourced);

            var sample = Sample.Create(visit.Id, testId);
            await _sampleRepository.AddAsync(sample, cancellationToken);
        }

        // 4. Single save for the entire transaction
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
