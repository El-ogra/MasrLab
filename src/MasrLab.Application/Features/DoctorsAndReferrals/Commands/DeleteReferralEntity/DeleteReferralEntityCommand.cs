using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.DeleteReferralEntity;

public record DeleteReferralEntityCommand(int Id) : IRequest<Unit>;
