namespace MasrLab.Application.Common.Interfaces;

public interface ICultureTemplateSeeder
{
    Task SeedFromTemplateAsync(int newCultureTestId, CancellationToken cancellationToken = default);
}
