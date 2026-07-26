using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddDoctor;

public class AddDoctorCommandHandler : IRequestHandler<AddDoctorCommand, Unit>
{
    public Task<Unit> Handle(AddDoctorCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
