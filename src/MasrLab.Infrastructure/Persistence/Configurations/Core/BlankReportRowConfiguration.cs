using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class BlankReportRowConfiguration : IEntityTypeConfiguration<BlankReportRow>
{
    public void Configure(EntityTypeBuilder<BlankReportRow> builder)
    {
        builder.ToTable("BlankReportRows");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.BlankReportId).IsRequired();
        builder.Property(e => e.TestName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Result).HasMaxLength(200);
        builder.Property(e => e.Unit).HasMaxLength(50);
        builder.Property(e => e.Flag).HasMaxLength(20);
        builder.Property(e => e.ReferenceRange).HasMaxLength(100);
        builder.Property(e => e.DisplayOrder).IsRequired();

        builder.HasIndex(e => new { e.BlankReportId, e.DisplayOrder });
    }
}
