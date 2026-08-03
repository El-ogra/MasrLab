using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class WorkSheetConfiguration : IEntityTypeConfiguration<WorkSheet>
{
    public void Configure(EntityTypeBuilder<WorkSheet> builder)
    {
        builder.ToTable("WorkSheets");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.OwnsOne(e => e.Period, p =>
        {
            p.Property(pr => pr.Start).HasColumnName("PeriodStart").IsRequired();
            p.Property(pr => pr.End).HasColumnName("PeriodEnd");
        });

        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.PatientVisitIds).HasMaxLength(2000);
        builder.Property(e => e.TestIds).HasMaxLength(2000);

        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.IsDeleted);
    }
}
