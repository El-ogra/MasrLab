using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Infrastructure.Persistence.Configurations.Administrative;

public class ReferralEntityConfiguration : IEntityTypeConfiguration<ReferralEntity>
{
    public void Configure(EntityTypeBuilder<ReferralEntity> builder)
    {
        builder.ToTable("ReferralEntities");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .IsRequired();

        builder.Property(e => e.ContactPerson)
            .HasMaxLength(100);

        builder.OwnsOne(e => e.ContactPhone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("ContactPhone").HasMaxLength(20);
        });

        builder.OwnsOne(e => e.Phone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20);
        });

        builder.Property(e => e.Fax)
            .HasMaxLength(20);

        builder.Property(e => e.Address)
            .HasMaxLength(200);

        builder.Property(e => e.PriceListId);

        builder.HasOne(e => e.PriceList)
            .WithMany()
            .HasForeignKey(e => e.PriceListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.AccountBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.Discount)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Commission)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.PriceListId);
        builder.HasIndex(e => e.IsDeleted);
    }
}
