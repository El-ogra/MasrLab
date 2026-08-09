namespace MasrLab.Application.Common.Helpers;

public interface IVisitLabIdGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
