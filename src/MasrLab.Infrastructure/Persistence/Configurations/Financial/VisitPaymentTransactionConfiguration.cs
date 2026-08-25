using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Infrastructure.Persistence.Configurations.Financial;

public class VisitPaymentTransactionConfiguration : IEntityTypeConfiguration<VisitPaymentTransaction>
{
    public void Configure(EntityTypeBuilder<VisitPaymentTransaction> builder)
    {
        builder.ToTable("VisitPaymentTransactions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.PaidDate).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.EditDate);

        builder.HasOne<Receipt>()
            .WithMany(r => r.Transactions)
            .HasForeignKey(e => e.ReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ReceiptId);
        builder.HasIndex(e => e.PaidDate);
        builder.HasIndex(e => e.IsDeleted);
    }
}
