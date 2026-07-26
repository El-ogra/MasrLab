using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Accounting.Queries.GetDrawerReport;

public record GetDrawerReportQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<AccountDrawerDto>;
