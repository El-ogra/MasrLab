using MasrLab.Application.Services;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using Xunit;

namespace MasrLab.Application.Tests;

public class TestComponentCardinalityServiceTests
{
    private readonly TestComponentCardinalityService _sut = new();

    [Fact]
    public async Task ValidateRemoveComponent_throws_when_last_component()
    {
        var test = new Test { Id = 1, Name = "CBC", Unit = "mg" };
        var component = TestComponent.Create(1, "CBC", "mg", 1);
        component.Id = 10;
        test.TestComponents.Add(component);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _sut.ValidateRemoveComponentAsync(test, component.Id));
    }

    [Fact]
    public async Task ValidateRemoveComponent_allows_when_multiple_components_exist()
    {
        var test = new Test { Id = 1, Name = "CBC", Unit = "mg" };
        var comp1 = TestComponent.Create(1, "WBC", "10^3/uL", 1);
        comp1.Id = 10;
        var comp2 = TestComponent.Create(1, "RBC", "10^6/uL", 2);
        comp2.Id = 11;
        test.TestComponents.Add(comp1);
        test.TestComponents.Add(comp2);

        await _sut.ValidateRemoveComponentAsync(test, comp1.Id);
    }

    [Fact]
    public async Task ValidateRemoveComponent_rejects_last_component_regardless_of_reference_values()
    {
        var test = new Test { Id = 1, Name = "CBC", Unit = "mg" };
        test.ReferenceValues = new List<ReferenceValue>();
        var component = TestComponent.Create(1, "CBC", "mg", 1);
        component.Id = 10;
        test.TestComponents.Add(component);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _sut.ValidateRemoveComponentAsync(test, component.Id));
    }
}
