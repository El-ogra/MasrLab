using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MasrLab.Application.Common.Behaviors;

public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IRequestAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

    public AuditBehavior(
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IRequestAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuditBehavior<TRequest, TResponse>> logger)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;
        var startTime = _dateTimeService.UtcNow;

        try
        {
            var response = await next();

            await PersistAuditEntryAsync(requestName, userId, startTime, success: true, errorMessage: null, cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            await PersistAuditEntryAsync(requestName, userId, startTime, success: false, errorMessage: ex.Message, cancellationToken);

            throw;
        }
    }

    private async Task PersistAuditEntryAsync(
        string requestName, int? userId, DateTime startTimeUtc,
        bool success, string? errorMessage, CancellationToken ct)
    {
        try
        {
            var entry = new RequestAuditLog
            {
                RequestName = requestName,
                UserId = userId,
                ActionTimeUtc = startTimeUtc,
                Success = success,
                ErrorMessage = errorMessage
            };

            await _auditLogRepository.AddAsync(entry, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Audit persistence failure must not abort the request.
            _logger.LogWarning(ex, "Failed to persist audit entry for request {RequestName}.", requestName);
        }
    }
}
