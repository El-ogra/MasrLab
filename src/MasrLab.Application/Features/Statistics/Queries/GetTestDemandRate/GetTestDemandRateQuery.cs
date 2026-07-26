using MediatR;
using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetTestDemandRate;

public record GetTestDemandRateQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<StatisticsDto>;
