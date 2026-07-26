using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Administrative;

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

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Fax)
            .HasMaxLength(20);

        builder.Property(e => e.Address)
            .HasMaxLength(200);

        builder.Property(e => e.PriceListId)
            .IsRequired();

        builder.Property(e => e.AccountBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.PriceListId);
    }
}
