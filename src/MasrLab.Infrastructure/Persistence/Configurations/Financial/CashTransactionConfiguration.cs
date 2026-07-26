using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Infrastructure.Persistence.Configurations.Financial;

public class CashTransactionConfiguration : IEntityTypeConfiguration<CashTransaction>
{
    public void Configure(EntityTypeBuilder<CashTransaction> builder)
    {
        builder.ToTable("CashTransactions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.EntityId).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.TransactionDate).IsRequired();

        builder.HasIndex(e => e.EntityId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.TransactionDate);
        builder.HasIndex(e => e.IsDeleted);
    }
}
