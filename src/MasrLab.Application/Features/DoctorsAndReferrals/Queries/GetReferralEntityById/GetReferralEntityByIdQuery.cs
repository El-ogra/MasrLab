using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetReferralEntityById;

public record GetReferralEntityByIdQuery(int Id) : IRequest<ReferralEntityDto?>;
