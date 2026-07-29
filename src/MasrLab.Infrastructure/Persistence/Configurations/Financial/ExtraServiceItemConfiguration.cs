using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Infrastructure.Persistence.Configurations.Financial;

public class ExtraServiceItemConfiguration : IEntityTypeConfiguration<ExtraServiceItem>
{
    public void Configure(EntityTypeBuilder<ExtraServiceItem> builder)
    {
        builder.ToTable("ExtraServiceItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ReceiptId).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(e => e.ReceiptId);
        builder.HasIndex(e => e.IsDeleted);
    }
}
