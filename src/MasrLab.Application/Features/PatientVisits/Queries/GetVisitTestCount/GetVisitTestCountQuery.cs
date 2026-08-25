using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Queries.GetVisitTestCount;

public sealed record GetVisitTestCountQuery(int PatientVisitId) : IRequest<int>;
