using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Persistence.Configurations.Culture;

public class CulturePrintReceiptConfiguration : IEntityTypeConfiguration<CulturePrintReceipt>
{
    public void Configure(EntityTypeBuilder<CulturePrintReceipt> builder)
    {
        builder.ToTable("CulturePrintReceipts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.VisitTestResultItemId).IsRequired();
        builder.Property(e => e.PrintedByUserId).IsRequired();
        builder.Property(e => e.PrintedAt).IsRequired();
        builder.Property(e => e.PrintCount).IsRequired();

        builder.HasOne<VisitTestResultItem>()
            .WithMany()
            .HasForeignKey(e => e.VisitTestResultItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.VisitTestResultItemId);
        builder.HasIndex(e => e.IsDeleted);
    }
}
