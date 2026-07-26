using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAttendanceLogs;

public record GetAttendanceLogsQuery(int UserId, DateTime PeriodStart, DateTime PeriodEnd) : IRequest<AttendanceDto>;
