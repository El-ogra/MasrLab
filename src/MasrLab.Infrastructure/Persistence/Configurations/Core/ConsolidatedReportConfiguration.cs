using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class ConsolidatedReportConfiguration : IEntityTypeConfiguration<ConsolidatedReport>
{
    public void Configure(EntityTypeBuilder<ConsolidatedReport> builder)
    {
        builder.ToTable("ConsolidatedReports");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PatientVisitId).IsRequired();
        builder.Property(e => e.PrintGroupSubtitles).IsRequired();
        builder.Property(e => e.Comment).HasMaxLength(1000);

        builder.HasIndex(e => e.PatientVisitId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(item => item.ConsolidatedReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
