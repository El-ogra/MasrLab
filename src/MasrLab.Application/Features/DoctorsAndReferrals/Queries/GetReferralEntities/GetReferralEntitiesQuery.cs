using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetReferralEntities;

public record GetReferralEntitiesQuery : IRequest<IReadOnlyList<ReferralEntityDto>>;
