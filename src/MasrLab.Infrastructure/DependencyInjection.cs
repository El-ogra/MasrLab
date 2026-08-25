using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Application.Services;
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

        // Application services
        services.AddScoped<ICultureTemplateSeeder, CultureTemplateSeeder>();

        // Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ISqlServerBackupExecutor, SqlServerBackupExecutor>();
        services.AddSingleton<IRestoreAccessModeRecovery, RestoreAccessModeRecovery>();
        services.AddSingleton<IPdfPrinter, WindowsPdfPrinter>();
        services.AddScoped<IPrintService, PrintService>();
        services.AddScoped<IReportDefinition, ReceiptReport>();
        services.AddScoped<IReportDefinition, EnvelopeReport>();
        services.AddScoped<IReportDefinition, IndividualResultReport>();
        services.AddScoped<IReportDefinition, CombinedReport>();
        services.AddScoped<IReportDefinition, BlankReport>();
        services.AddScoped<IReportDefinition, CultureReport>();
        services.AddScoped<IReportDefinition, WorkSheetReport>(); services.AddScoped<IReportDefinition, PatientHistoryReport>(); services.AddScoped<IReportDefinition, AttendanceReport>(); services.AddScoped<IReportDefinition, PriceListReport>(); services.AddScoped<IReportDefinition, DrawerReport>(); services.AddScoped<IReportDefinition, StatisticsReport>();
        services.AddScoped<ReportDefinitionRegistry>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IBarcodeService, BarcodeService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<ITestResultRepository, TestResultRepository>();
        services.AddScoped<ICultureRepository, CultureRepository>();
        services.AddScoped<IVisitTestResultItemRepository, VisitTestResultItemRepository>();
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
        services.AddScoped<ITestGroupRepository, TestGroupRepository>();
        services.AddScoped<IAntibioticRepository, AntibioticRepository>();
        services.AddScoped<ICultureAntibioticRepository, CultureAntibioticRepository>();
        services.AddScoped<IPriceListRepository, PriceListRepository>();
        services.AddScoped<IReferralEntityRepository, ReferralEntityRepository>();
        services.AddScoped<ITestRepository, TestRepository>();
        services.AddScoped<ICommercialPackageRepository, CommercialPackageRepository>();
        services.AddScoped<ITestComponentChoiceRepository, TestComponentChoiceRepository>();
        services.AddScoped<IReceiptPrintDataReader, ReceiptPrintDataReader>();
        services.AddScoped<IVisitAccountReader, VisitAccountReader>();
        services.AddScoped<IWorklistReader, WorklistReader>();
        services.AddScoped<IReprintWarningReader, ReprintWarningReader>();
        services.AddScoped<IBlankReportReader, BlankReportReader>();
        services.AddScoped<IClinicalReportReader, ClinicalReportReader>();
        services.AddScoped<IEnvelopePrintDataReader, EnvelopePrintDataReader>();
        services.AddScoped<IOperationalReportDataReader, OperationalReportPrintDataReader>();

        return services;
    }
}
