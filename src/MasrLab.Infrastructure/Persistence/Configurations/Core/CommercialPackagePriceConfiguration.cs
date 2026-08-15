using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class CommercialPackagePriceConfiguration : IEntityTypeConfiguration<CommercialPackagePrice>
{
    public void Configure(EntityTypeBuilder<CommercialPackagePrice> builder)
    {
        builder.ToTable("CommercialPackagePrices");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CommercialPackageId).IsRequired();
        builder.Property(e => e.PriceListId).IsRequired();
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(e => e.CommercialPackageId);
        builder.HasIndex(e => e.PriceListId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasIndex(e => new { e.CommercialPackageId, e.PriceListId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
