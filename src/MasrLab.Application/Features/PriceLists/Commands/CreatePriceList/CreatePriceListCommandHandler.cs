using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;

public class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, Unit>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePriceListCommandHandler(
        IRepository<PriceList> repository,
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
    {
        if (request.IsDefault)
        {
            await ClearDefaultFlagAsync(cancellationToken);
        }

        var priceList = new PriceList
        {
            Name = request.Name,
            IsDefault = request.IsDefault
        };

        await _repository.AddAsync(priceList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task ClearDefaultFlagAsync(CancellationToken cancellationToken)
    {
        var currentDefault = await _priceListRepository.GetDefaultAsync(cancellationToken);
        if (currentDefault is not null)
        {
            currentDefault.IsDefault = false;
            _repository.Update(currentDefault);
        }
    }
}
