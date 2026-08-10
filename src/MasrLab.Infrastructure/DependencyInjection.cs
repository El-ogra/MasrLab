using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Interceptors;
using MasrLab.Infrastructure.Persistence.Repositories;
using MasrLab.Infrastructure.Persistence.Readers;
using MasrLab.Infrastructure.Printing;
using MasrLab.Infrastructure.Printing.Reports;
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
            var connectionString = ConnectionStringBuilder.GetProductionConnectionString(configuration);
            optionsBuilder.UseSqlServer(connectionString);
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
        services.AddSingleton<IReportDefinition, ReceiptReport>();
        services.AddSingleton<IReportDefinition, EnvelopeReport>();
        services.AddSingleton<IReportDefinition, IndividualResultReport>();
        services.AddSingleton<IReportDefinition, CombinedReport>();
        services.AddSingleton<IReportDefinition, BlankReport>();
        services.AddSingleton<IReportDefinition, CultureReport>();
        services.AddSingleton<ReportDefinitionRegistry>();
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
        services.AddScoped<IStatisticsSettingsRepository, StatisticsSettingsRepository>();
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
        services.AddScoped<IReceiptPrintDataReader, ReceiptPrintDataReader>();
        services.AddScoped<IEnvelopePrintDataReader, EnvelopePrintDataReader>();

        return services;
    }
}
