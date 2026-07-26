using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.AddAntibioticToCulture;

public class AddAntibioticToCultureCommandHandler : IRequestHandler<AddAntibioticToCultureCommand, Unit>
{
    public Task<Unit> Handle(AddAntibioticToCultureCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
