using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _auditLogRepository.GetByPeriodAsync(
            request.PeriodStart,
            request.PeriodEnd,
            request.UserId,
            cancellationToken);

        var filtered = string.IsNullOrWhiteSpace(request.EntityType)
            ? logs
            : logs.Where(l => l.EntityType.ToString() == request.EntityType).ToList();

        return filtered.Select(l => _mapper.Map<AuditLogDto>(l)).ToList();
    }
}
