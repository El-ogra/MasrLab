using MediatR;
using System.Diagnostics;

namespace MasrLab.Application.Common.Behaviors;

public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"[Audit] {typeof(TRequest).Name} - {DateTime.UtcNow:O}");
        return await next();
    }
}
