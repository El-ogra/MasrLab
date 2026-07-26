using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;

public class AddReferralEntityCommandHandler : IRequestHandler<AddReferralEntityCommand, Unit>
{
    public Task<Unit> Handle(AddReferralEntityCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
