using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class BlankReportConfiguration : IEntityTypeConfiguration<BlankReport>
{
    public void Configure(EntityTypeBuilder<BlankReport> builder)
    {
        builder.ToTable("BlankReports");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PatientVisitId).IsRequired();
        builder.Property(e => e.ReportTitle).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Comment).HasMaxLength(1000);
        builder.Property(e => e.PaginationNote).HasMaxLength(200);

        builder.HasIndex(e => e.PatientVisitId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasMany(r => r.Rows)
            .WithOne()
            .HasForeignKey(row => row.BlankReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
