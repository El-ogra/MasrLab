using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class ConsolidatedReportItemConfiguration : IEntityTypeConfiguration<ConsolidatedReportItem>
{
    public void Configure(EntityTypeBuilder<ConsolidatedReportItem> builder)
    {
        builder.ToTable("ConsolidatedReportItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ConsolidatedReportId).IsRequired();
        builder.Property(e => e.VisitTestId).IsRequired();
        builder.Property(e => e.DisplayOrder).IsRequired();

        // Plan M-8: a visit test may appear only once per consolidated report.
        builder.HasIndex(e => new { e.ConsolidatedReportId, e.VisitTestId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => new { e.ConsolidatedReportId, e.DisplayOrder });
    }
}
