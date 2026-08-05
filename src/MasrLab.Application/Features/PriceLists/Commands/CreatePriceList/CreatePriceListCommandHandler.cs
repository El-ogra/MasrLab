using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;

public class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, Unit>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePriceListCommandHandler(IRepository<PriceList> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = new PriceList
        {
            Name = request.Name
        };

        await _repository.AddAsync(priceList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
