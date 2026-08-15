using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

public class TestComponentCardinalityService : ITestComponentCardinalityService
{
    public Task ValidateAddComponentAsync(Test test, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task ValidateRemoveComponentAsync(Test test, int componentId, CancellationToken ct = default)
    {
        var activeComponents = test.TestComponents.Where(c => c.Id != componentId).ToList();
        if (activeComponents.Count == 0)
        {
            throw new BusinessRuleViolationException(
                "لا يمكن حذف المكون الأخير - يجب أن يحتوي التحليل على مكون واحد على الأقل");
        }
        return Task.CompletedTask;
    }
}
