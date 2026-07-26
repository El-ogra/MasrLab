using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Accounting.Queries.GetDrawerReport;

public record GetDrawerReportQuery : IRequest<AccountDrawerDto>;
