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
        services.AddScoped<IVisitLabIdGenerator, VisitLabIdGenerator>();
        services.AddScoped<Common.Interfaces.ICultureTemplateSeeder, NullCultureTemplateSeeder>();

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
        services.AddScoped<IReferenceValueMatcher, ReferenceValueMatcher>();
        services.AddScoped<IVisitCompletionEvaluator, VisitCompletionEvaluator>();
        services.AddScoped<ISampleTrackingService, SampleTrackingService>();
        services.AddScoped<ITestComponentCardinalityService, TestComponentCardinalityService>();
        services.AddScoped<IVisitTestSnapshotter, VisitTestSnapshotter>();

        return services;
    }
}
