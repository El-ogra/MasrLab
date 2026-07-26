using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class WorkSheetConfiguration : IEntityTypeConfiguration<WorkSheet>
{
    public void Configure(EntityTypeBuilder<WorkSheet> builder)
    {
        builder.ToTable("WorkSheets");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PeriodStart).IsRequired();
        builder.Property(e => e.PeriodEnd).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.PatientVisitIds).HasMaxLength(2000);
        builder.Property(e => e.TestIds).HasMaxLength(2000);

        builder.HasIndex(e => e.PeriodStart);
        builder.HasIndex(e => e.PeriodEnd);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.IsDeleted);
    }
}
