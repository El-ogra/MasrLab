using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class CommercialPackageItemConfiguration : IEntityTypeConfiguration<CommercialPackageItem>
{
    public void Configure(EntityTypeBuilder<CommercialPackageItem> builder)
    {
        builder.ToTable("CommercialPackageItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CommercialPackageId).IsRequired();
        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.DisplayOrder).IsRequired();

        builder.HasIndex(e => e.CommercialPackageId);
        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasIndex(e => new { e.CommercialPackageId, e.TestId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => new { e.CommercialPackageId, e.DisplayOrder })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
