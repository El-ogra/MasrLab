using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.CreateDoctorDrawer;

public class CreateDoctorDrawerCommandHandler : IRequestHandler<CreateDoctorDrawerCommand, Unit>
{
    public Task<Unit> Handle(CreateDoctorDrawerCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
