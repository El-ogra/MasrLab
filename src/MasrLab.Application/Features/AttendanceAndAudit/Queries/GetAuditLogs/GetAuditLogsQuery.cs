using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAuditLogs;

public record GetAuditLogsQuery(int? UserId, string? EntityType, DateTime PeriodStart, DateTime PeriodEnd) : IRequest<object>;
