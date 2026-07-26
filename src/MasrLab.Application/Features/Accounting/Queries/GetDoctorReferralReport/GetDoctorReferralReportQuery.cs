using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Accounting.Queries.GetDoctorReferralReport;

public record GetDoctorReferralReportQuery : IRequest<AccountDrawerDto>;
