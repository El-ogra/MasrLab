using MediatR;

namespace MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;

public class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, Unit>
{
    public Task<Unit> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
