using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;

public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, Unit>
{
    public Task<Unit> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
