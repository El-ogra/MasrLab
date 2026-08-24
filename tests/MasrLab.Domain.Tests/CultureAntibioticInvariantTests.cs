using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class CultureAntibioticInvariantTests
{
    [Fact]
    public void Create_WhenValidIds_ShouldDefaultApplicabilityFlagsToFalse()
    {
        var assignment = CultureAntibiotic.Create(10, 20);

        Assert.Equal(10, assignment.CultureTestId);
        Assert.Equal(20, assignment.AntibioticId);
        Assert.False(assignment.Pregnant);
        Assert.False(assignment.Children);
        Assert.Empty(assignment.CommercialNames);
    }

    [Fact]
    public void Create_WhenCultureTestIdIsZero_ShouldThrowBusinessRuleViolation()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => CultureAntibiotic.Create(0, 20));

        Assert.Equal("CultureAntibiotic requires a valid CultureTestId.", exception.Message);
    }

    [Fact]
    public void Create_WhenCultureTestIdIsNegative_ShouldThrowBusinessRuleViolation()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => CultureAntibiotic.Create(-1, 20));

        Assert.Equal("CultureAntibiotic requires a valid CultureTestId.", exception.Message);
    }

    [Fact]
    public void Create_WhenAntibioticIdIsZero_ShouldThrowBusinessRuleViolation()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => CultureAntibiotic.Create(10, 0));

        Assert.Equal("CultureAntibiotic requires a valid AntibioticId.", exception.Message);
    }

    [Fact]
    public void Create_WhenAntibioticIdIsNegative_ShouldThrowBusinessRuleViolation()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => CultureAntibiotic.Create(10, -1));

        Assert.Equal("CultureAntibiotic requires a valid AntibioticId.", exception.Message);
    }
}
