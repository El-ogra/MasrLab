using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAttendanceLogs;

public class GetAttendanceLogsQueryHandler : IRequestHandler<GetAttendanceLogsQuery, AttendanceDto>
{
    private readonly IRepository<AttendanceLog> _repository;
    private readonly IMapper _mapper;

    public GetAttendanceLogsQueryHandler(IRepository<AttendanceLog> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AttendanceDto> Handle(GetAttendanceLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _repository.GetAllAsync(cancellationToken);

        var filtered = logs
            .Where(l => l.UserId == request.UserId
                     && l.WorkPeriod.Start >= request.PeriodStart
                     && l.WorkPeriod.End <= request.PeriodEnd)
            .ToList();

        var log = filtered.FirstOrDefault()!;

        return _mapper.Map<AttendanceDto>(log);
    }
}
