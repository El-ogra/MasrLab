using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;
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
        IEnumerable<AuditLog> logs;

        if (request.UserId.HasValue)
        {
            logs = await _auditLogRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        }
        else
        {
            logs = await _auditLogRepository.GetAllAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            logs = logs.Where(l => l.EntityType.ToString() == request.EntityType);
        }

        logs = logs.Where(l => l.ActionTime >= request.PeriodStart && l.ActionTime <= request.PeriodEnd);

        return logs.Select(l => _mapper.Map<AuditLogDto>(l)).ToList();
    }
}
