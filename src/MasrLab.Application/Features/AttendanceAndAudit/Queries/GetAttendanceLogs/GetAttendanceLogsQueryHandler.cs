using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAttendanceLogs;

public class GetAttendanceLogsQueryHandler : IRequestHandler<GetAttendanceLogsQuery, AttendanceDto>
{
    private readonly IRepository<AttendanceLog> _repository;

    public GetAttendanceLogsQueryHandler(IRepository<AttendanceLog> repository)
    {
        _repository = repository;
    }

    public async Task<AttendanceDto> Handle(GetAttendanceLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _repository.GetAllAsync();

        var filtered = logs
            .Where(l => l.UserId == request.UserId
                     && l.WorkPeriod.Start >= request.PeriodStart
                     && l.WorkPeriod.End <= request.PeriodEnd)
            .ToList();

        var log = filtered.FirstOrDefault()!;

        return new AttendanceDto
        {
            Id = log.Id,
            UserId = log.UserId,
            LoginTime = log.WorkPeriod.Start,
            LogoutTime = log.WorkPeriod.End,
            Overtime = log.Overtime,
            Delays = log.Delays,
            BreakPeriods = log.BreakPeriods
        };
    }
}
