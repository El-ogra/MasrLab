using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAuditLogs;

public record GetAuditLogsQuery : IRequest<object>;
