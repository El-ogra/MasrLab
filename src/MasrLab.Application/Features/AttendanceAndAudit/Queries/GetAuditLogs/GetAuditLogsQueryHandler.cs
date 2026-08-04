using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    public Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
