using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Infrastructure.Persistence.Configurations.Settings;

public class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> builder)
    {
        builder.ToTable("PriceLists");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.IsDefault).IsRequired();

        // Filtered unique index: only one PriceList can have IsDefault = true at a time.
        // The filter ensures the unique constraint applies only to rows where IsDefault = 1,
        // so multiple rows with IsDefault = 0 are allowed without conflict.
        builder.HasIndex(e => e.IsDefault)
            .IsUnique()
            .HasFilter("[IsDefault] = 1");

        builder.HasIndex(e => e.IsDeleted);
    }
}
