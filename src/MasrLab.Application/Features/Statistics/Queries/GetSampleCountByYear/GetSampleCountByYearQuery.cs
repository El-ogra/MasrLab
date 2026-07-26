using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetSampleCountByYear;

public record GetSampleCountByYearQuery : IRequest<StatisticsDto>;
