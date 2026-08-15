using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class VisitTestResultItemConfiguration : IEntityTypeConfiguration<VisitTestResultItem>
{
    public void Configure(EntityTypeBuilder<VisitTestResultItem> builder)
    {
        builder.ToTable("VisitTestResultItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.VisitTestId).IsRequired();
        builder.Property(e => e.SourceTestComponentId).IsRequired();
        builder.Property(e => e.ComponentName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ComponentUnit).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DisplayOrder).IsRequired();
        builder.Property(e => e.ResultEntryKind).IsRequired();

        builder.HasOne<TestComponent>()
            .WithMany()
            .HasForeignKey(e => e.SourceTestComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.VisitTestId);
        builder.HasIndex(e => e.SourceTestComponentId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasIndex(e => new { e.VisitTestId, e.SourceTestComponentId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
