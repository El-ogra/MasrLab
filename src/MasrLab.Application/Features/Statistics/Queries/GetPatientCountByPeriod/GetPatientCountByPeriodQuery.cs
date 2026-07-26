using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetPatientCountByPeriod;

public record GetPatientCountByPeriodQuery : IRequest<StatisticsDto>;
