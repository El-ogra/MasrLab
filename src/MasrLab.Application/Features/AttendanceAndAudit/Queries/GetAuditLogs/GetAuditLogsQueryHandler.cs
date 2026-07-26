using MediatR;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, object>
{
    public Task<object> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
