using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Common;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Infrastructure.Persistence.Views;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MasrLab.Infrastructure.Persistence;

public class MasrLabDbContext : DbContext
{
    public MasrLabDbContext(DbContextOptions<MasrLabDbContext> options)
        : base(options)
    {
    }

    // Core
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientVisit> PatientVisits => Set<PatientVisit>();
    public DbSet<VisitTest> VisitTests => Set<VisitTest>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<ReferenceValue> ReferenceValues => Set<ReferenceValue>();
    public DbSet<TestGroup> TestGroups => Set<TestGroup>();
    public DbSet<TestGroupItem> TestGroupItems => Set<TestGroupItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<SampleCollection> SampleCollections => Set<SampleCollection>();

    // Culture
    public DbSet<Domain.Entities.Culture.Culture> Cultures => Set<Domain.Entities.Culture.Culture>();
    public DbSet<Organism> Organisms => Set<Organism>();
    public DbSet<Antibiotic> Antibiotics => Set<Antibiotic>();
    public DbSet<Sensitivity> Sensitivities => Set<Sensitivity>();

    // Financial
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();
    public DbSet<OutsourcedSample> OutsourcedSamples => Set<OutsourcedSample>();
    public DbSet<ExtraServiceItem> ExtraServiceItems => Set<ExtraServiceItem>();
    public DbSet<ExternalLab> ExternalLabs => Set<ExternalLab>();

    // Administrative
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<ReferralEntity> ReferralEntities => Set<ReferralEntity>();
    public DbSet<CommentTemplate> CommentTemplates => Set<CommentTemplate>();

    // Settings
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<Printer> Printers => Set<Printer>();
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
    public DbSet<WorkSheet> WorkSheets => Set<WorkSheet>();
    public DbSet<CardSetting> CardSettings => Set<CardSetting>();

    // Views
    public DbSet<PatientHistoryView> PatientHistoryViews => Set<PatientHistoryView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MasrLabDbContext).Assembly);

        modelBuilder.Entity<PatientHistoryView>().HasNoKey().ToView("PatientHistoryView");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var comparison = Expression.Equal(property, falseConstant);
                var lambda = Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
