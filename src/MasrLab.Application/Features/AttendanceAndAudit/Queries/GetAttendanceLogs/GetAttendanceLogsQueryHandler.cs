using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAttendanceLogs;

public class GetAttendanceLogsQueryHandler : IRequestHandler<GetAttendanceLogsQuery, AttendanceDto>
{
    public Task<AttendanceDto> Handle(GetAttendanceLogsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
