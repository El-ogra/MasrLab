using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCasesByPeriod;

public record GetCasesByPeriodQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<IReadOnlyList<VisitDto>>;
