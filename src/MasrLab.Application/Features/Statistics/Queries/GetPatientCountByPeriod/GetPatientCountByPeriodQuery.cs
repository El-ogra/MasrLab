using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetPatientCountByPeriod;

public record GetPatientCountByPeriodQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<PatientCountByPeriodDto>;
