using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;

public class UpdatePriceListNameCommandHandler : IRequestHandler<UpdatePriceListNameCommand, Unit>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePriceListNameCommandHandler(IRepository<PriceList> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdatePriceListNameCommand request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            throw new EntityNotFoundException(nameof(PriceList), request.PriceListId);

        priceList.Name = request.Name;
        _repository.Update(priceList);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
