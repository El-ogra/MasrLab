using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.AttendanceAndAudit.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<AuditLog> logs;

        if (request.UserId.HasValue)
        {
            logs = await _auditLogRepository.GetByUserIdAsync(request.UserId.Value);
        }
        else
        {
            logs = await _auditLogRepository.GetAllAsync();
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            logs = logs.Where(l => l.EntityType.ToString() == request.EntityType);
        }

        logs = logs.Where(l => l.ActionTime >= request.PeriodStart && l.ActionTime <= request.PeriodEnd);

        return logs.Select(l => new AuditLogDto
        {
            Id = l.Id,
            UserId = l.UserId,
            ActionType = l.ActionType,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            ActionTime = l.ActionTime,
            PrintCount = l.PrintCount
        }).ToList();
    }
}
