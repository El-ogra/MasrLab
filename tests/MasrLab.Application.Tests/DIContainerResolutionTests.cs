using FluentValidation;
using MasrLab.Application;
using MasrLab.Application.Common.Helpers;
using MasrLab.Domain.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MasrLab.Application.Tests;

public class DIContainerResolutionTests
{
    [Fact]
    public void AddApplication_resolves_every_handler_validator_and_its_ten_domain_services()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
        services.AddSingleton<ILoggerFactory>(loggerFactory.Object);
        RegisterInfrastructureSubstitutesRequiredOnlyForApplicationResolution(services);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        var assembly = typeof(DependencyInjection).Assembly;
        var handlerContracts = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .Distinct()
            .ToList();

        foreach (var handlerContract in handlerContracts)
            Assert.NotNull(provider.GetRequiredService(handlerContract));

        foreach (var validator in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract &&
                     t.BaseType is { IsGenericType: true } baseType && baseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>)))
            Assert.NotNull(provider.GetRequiredService(typeof(IValidator<>).MakeGenericType(validator.BaseType!.GenericTypeArguments[0])));

        var applicationServices = new[]
        {
            typeof(IAccountingService), typeof(ICultureSensitivityService), typeof(IMedicalHistoryService),
            typeof(IOutsourcingService), typeof(IPriceListResolverService), typeof(IPricingService),
            typeof(IReceiptCalculationService), typeof(IReferralCommissionService), typeof(IResultValidationService),
            typeof(ISampleTrackingService), typeof(IVisitLabIdGenerator), typeof(LabIdGenerator)
        };
        foreach (var service in applicationServices)
            Assert.NotNull(provider.GetRequiredService(service));
    }

    private static void RegisterInfrastructureSubstitutesRequiredOnlyForApplicationResolution(IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        var dependencies = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetConstructors())
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType)
            .Where(t => !t.ContainsGenericParameters && (t.IsInterface || t.IsAbstract) && services.All(d => d.ServiceType != t))
            .Distinct();

        foreach (var dependency in dependencies)
        {
            var mock = Activator.CreateInstance(typeof(Mock<>).MakeGenericType(dependency))!;
            var value = mock.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly)
                .Single(p => p.Name == "Object").GetValue(mock)!;
            services.AddSingleton(dependency, value);
        }
    }
}
