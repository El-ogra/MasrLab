using MediatR;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCaseUserTracking;

public record GetCaseUserTrackingQuery : IRequest<IReadOnlyList<object>>;
