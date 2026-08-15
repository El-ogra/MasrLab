using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Services;

public interface ITestComponentCardinalityService
{
    Task ValidateAddComponentAsync(Test test, CancellationToken ct = default);
    Task ValidateRemoveComponentAsync(Test test, int componentId, CancellationToken ct = default);
}
