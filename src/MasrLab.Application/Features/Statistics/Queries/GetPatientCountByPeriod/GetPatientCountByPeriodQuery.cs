using MediatR;
using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetPatientCountByPeriod;

public record GetPatientCountByPeriodQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<StatisticsDto>;
