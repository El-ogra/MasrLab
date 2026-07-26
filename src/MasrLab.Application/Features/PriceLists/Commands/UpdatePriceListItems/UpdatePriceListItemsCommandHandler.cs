using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItems;

public class UpdatePriceListItemsCommandHandler : IRequestHandler<UpdatePriceListItemsCommand, Unit>
{
    public Task<Unit> Handle(UpdatePriceListItemsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
