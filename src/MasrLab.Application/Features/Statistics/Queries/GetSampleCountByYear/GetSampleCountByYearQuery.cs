using MediatR;
using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetSampleCountByYear;

public record GetSampleCountByYearQuery(int Year) : IRequest<StatisticsDto>;
