using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAttendanceLogs;

public class GetAttendanceLogsQueryHandler : IRequestHandler<GetAttendanceLogsQuery, AttendanceDto>
{
    private readonly IAttendanceLogRepository _repository;
    private readonly IMapper _mapper;

    public GetAttendanceLogsQueryHandler(IAttendanceLogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AttendanceDto> Handle(GetAttendanceLogsQuery request, CancellationToken cancellationToken)
    {
        var log = await _repository.GetByUserAndPeriodAsync(request.UserId, request.PeriodStart, request.PeriodEnd, cancellationToken);

        return _mapper.Map<AttendanceDto>(log!);
    }
}
