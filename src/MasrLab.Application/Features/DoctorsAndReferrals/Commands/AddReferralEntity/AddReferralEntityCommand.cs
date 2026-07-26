using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;

public record AddReferralEntityCommand : IRequest<Unit>;
