using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Interceptors;
using MasrLab.Infrastructure.Persistence.Repositories;
using MasrLab.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        // DbContext
        services.AddScoped<MasrLabDbContext>(provider =>
        {
            var interceptors = provider.GetServices<SaveChangesInterceptor>();
            var optionsBuilder = new DbContextOptionsBuilder<MasrLabDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            foreach (var interceptor in interceptors)
            {
                optionsBuilder.AddInterceptors(interceptor);
            }
            return new MasrLabDbContext(optionsBuilder.Options);
        });

        // UnitOfWork
        services.AddScoped<IUnitOfWork>(provider =>
        {
            var context = provider.GetRequiredService<MasrLabDbContext>();
            return new UnitOfWork(context);
        });

        // Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ISqlServerBackupExecutor, SqlServerBackupExecutor>();
        services.AddSingleton<IRestoreAccessModeRecovery, RestoreAccessModeRecovery>();
        services.AddSingleton<IPdfPrinter, WindowsPdfPrinter>();
        services.AddScoped<IPrintService, PrintService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IBarcodeService, BarcodeService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<ITestResultRepository, TestResultRepository>();
        services.AddScoped<ICultureRepository, CultureRepository>();
        services.AddScoped<IAccountingRepository, AccountingRepository>();
        services.AddScoped<IStatisticsRepository, StatisticsRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRequestAuditLogRepository, RequestAuditLogRepository>();
        services.AddScoped<IPatientHistoryRepository, PatientHistoryRepository>();
        services.AddScoped<IReferenceValueRepository, ReferenceValueRepository>();
        services.AddScoped<IPriceListItemRepository, PriceListItemRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IOutsourcedSampleRepository, OutsourcedSampleRepository>();
        services.AddScoped<IAttendanceLogRepository, AttendanceLogRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<ISampleRepository, SampleRepository>();
        services.AddScoped<ITestGroupItemRepository, TestGroupItemRepository>();
        services.AddScoped<IAntibioticRepository, AntibioticRepository>();
        services.AddScoped<IPriceListRepository, PriceListRepository>();

        return services;
    }
}
