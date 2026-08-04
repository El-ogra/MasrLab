using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCaseUserTracking;

public record GetCaseUserTrackingQuery(int UserId, DateTime PeriodStart, DateTime PeriodEnd) : IRequest<IReadOnlyList<CaseUserTrackingDto>>;
