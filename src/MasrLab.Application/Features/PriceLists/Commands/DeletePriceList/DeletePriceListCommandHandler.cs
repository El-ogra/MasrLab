using MediatR;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;

public class DeletePriceListCommandHandler : IRequestHandler<DeletePriceListCommand, Unit>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePriceListCommandHandler(IRepository<PriceList> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeletePriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PriceList), request.PriceListId);

        if (priceList.IsDefault)
        {
            priceList.IsDefault = false;
        }

        priceList.IsDeleted = true;
        _repository.Update(priceList);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
