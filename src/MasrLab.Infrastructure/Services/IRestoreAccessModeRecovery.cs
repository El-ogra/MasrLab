namespace MasrLab.Infrastructure.Services;

public interface IRestoreAccessModeRecovery
{
    Task EnsureMultiUserIfNeededAsync(CancellationToken ct = default);
}
