using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientAccount;

public class UpdatePatientAccountCommandHandler : IRequestHandler<UpdatePatientAccountCommand, Unit>
{
    public Task<Unit> Handle(UpdatePatientAccountCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
