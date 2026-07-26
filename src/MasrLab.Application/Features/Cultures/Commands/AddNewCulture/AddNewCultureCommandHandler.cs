using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.AddNewCulture;

public class AddNewCultureCommandHandler : IRequestHandler<AddNewCultureCommand, Unit>
{
    public Task<Unit> Handle(AddNewCultureCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
