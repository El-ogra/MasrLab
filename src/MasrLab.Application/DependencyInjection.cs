using System.Reflection;
using FluentValidation;
using MasrLab.Application.Common.Helpers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(cfg => { }, assembly);
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.ValidationBehavior<,>));
        services.AddScoped<LabIdGenerator>();

        return services;
    }
}
