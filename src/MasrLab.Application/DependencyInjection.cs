using System.Reflection;
using FluentValidation;
using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Services;
using MasrLab.Domain.Services;
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
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.AuditBehavior<,>));
        services.AddScoped<LabIdGenerator>();

        // Domain services
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<ICultureSensitivityService, CultureSensitivityService>();
        services.AddScoped<IMedicalHistoryService, MedicalHistoryService>();
        services.AddScoped<IOutsourcingService, OutsourcingService>();
        services.AddScoped<IPriceListResolverService, PriceListResolverService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IReceiptCalculationService, ReceiptCalculationService>();
        services.AddScoped<IReferralCommissionService, ReferralCommissionService>();
        services.AddScoped<IResultValidationService, ResultValidationService>();
        services.AddScoped<ISampleTrackingService, SampleTrackingService>();

        return services;
    }
}
