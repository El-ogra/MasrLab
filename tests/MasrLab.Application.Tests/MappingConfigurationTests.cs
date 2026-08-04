using AutoMapper;
using MasrLab.Application.Common.Mappings.Profiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace MasrLab.Application.Tests;

public class MappingConfigurationTests
{
    [Fact]
    public void AutoMapper_Configuration_ShouldBeValid()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddMaps(typeof(DependencyInjection).Assembly);

        var configuration = new MapperConfiguration(expression, NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
