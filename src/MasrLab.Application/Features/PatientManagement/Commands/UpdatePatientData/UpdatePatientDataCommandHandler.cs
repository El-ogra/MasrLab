using MediatR;

namespace MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;

public class UpdatePatientDataCommandHandler : IRequestHandler<UpdatePatientDataCommand, Unit>
{
    public Task<Unit> Handle(UpdatePatientDataCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
