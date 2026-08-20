using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetExternalLabCandidates;

public record GetExternalLabCandidatesQuery : IRequest<IReadOnlyList<ReferralEntityDto>>;
