using MasrLab.Application.Common.Interfaces;

namespace MasrLab.Application.Services;

public sealed class NullCultureTemplateSeeder : ICultureTemplateSeeder
{
    public Task SeedFromTemplateAsync(int newCultureTestId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
