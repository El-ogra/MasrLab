using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetSampleCountByYear;

public record GetSampleCountByYearQuery(int Year) : IRequest<SampleCountByYearDto>;
