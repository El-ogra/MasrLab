using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Infrastructure.Persistence.Configurations.Financial;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PatientVisitId).IsRequired();
        builder.Property(e => e.Total).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.Discount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
        builder.Property(e => e.PaidPrevious).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.PaidNow).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.Remaining).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.ChangeDue).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.RefundToPatient).IsRequired();
        builder.Property(e => e.IssueDate).IsRequired();
        builder.Property(e => e.ReceiveTime).IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(10).IsRequired();

        builder.HasIndex(e => e.PatientVisitId);
        builder.HasIndex(e => e.IssueDate);
        builder.HasIndex(e => e.IsDeleted);
    }
}
