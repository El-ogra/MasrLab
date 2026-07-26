using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Repositories;
using MasrLab.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPrintService, PrintService>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<ITestResultRepository, TestResultRepository>();
        services.AddScoped<ICultureRepository, CultureRepository>();
        services.AddScoped<IAccountingRepository, AccountingRepository>();
        services.AddScoped<IStatisticsRepository, StatisticsRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}
